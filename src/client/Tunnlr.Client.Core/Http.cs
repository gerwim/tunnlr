using System.Text;
using Tunnlr.Client.Core.Models;
using Tunnlr.Common.Exceptions;
using Tunnlr.Common.Protobuf;
using HttpMethod = Tunnlr.Common.Protobuf.HttpMethod;

namespace Tunnlr.Client.Core;

public static class Http
{
    private static readonly HttpClient HttpClient = new(new HttpClientHandler
    {
        ClientCertificateOptions = ClientCertificateOption.Manual,
        ServerCertificateCustomValidationCallback = (_, _, _, _) => true, // TODO: allow configuration per tunnel
    });
    
    public static async Task<HttpInvokeRequestResult> InvokeRequest(HttpRequest request, Stream body,
        CancellationToken cancellationToken, IServiceProvider serviceProvider)
    {
        try
        {
            var requestMessage = new HttpRequestMessage
            {
                RequestUri = new Uri(request.TargetUri),
                Method = request.HttpMethod switch
                {
                    HttpMethod.Delete => System.Net.Http.HttpMethod.Delete,
                    HttpMethod.Post => System.Net.Http.HttpMethod.Post,
                    HttpMethod.Head => System.Net.Http.HttpMethod.Head,
                    HttpMethod.Options => System.Net.Http.HttpMethod.Options,
                    HttpMethod.Get => System.Net.Http.HttpMethod.Get,
                    HttpMethod.Put => System.Net.Http.HttpMethod.Put,
                    HttpMethod.Trace => System.Net.Http.HttpMethod.Trace,
                    _ => throw new InvalidHttpMethodException($"Invalid HttpMethod specified: {request.HttpMethod}")
                }
            };
            if (request.ContainsBody)
            {
                var content = new StreamContent(body, 8192);
                requestMessage.Content = content;
            }

            foreach (var header in request.Headers)
            {
                if (header.Key.ToLowerInvariant() == "host") continue;

                if (header.Key.ToLowerInvariant().StartsWith("content-"))
                {
                    requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
                else
                {
                    requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            var result = await HttpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

            var response = new HttpResponse
            {
                HttpRequestId = request.Id,
                StatusCode = (int)result.StatusCode,
                // TODO: cookies
            };
            
            Common.Helpers.MapHeaders(result.Headers, result.Content.Headers, response.Headers);
            
            return new HttpInvokeRequestResult
            {
                Response = response,
                Stream = await result.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false)
            };
        }
        catch (Exception ex)
        {
            var response = new HttpResponse
            {
                HttpRequestId = request.Id,
                StatusCode = 500,
            };
            
            
            response.Headers.Add("Content-Type", "text/html; charset=UTF-8");
            
            return new HttpInvokeRequestResult
            {
                Response = response,
                Stream = new MemoryStream(Encoding.UTF8.GetBytes(GenerateErrorPage(ex)))
            };
        }
    }
    
    private static string GenerateErrorPage(Exception ex)
    {
        return $"""""
        <html lang="en">
        <head>
            <title>Tunnlr error</title>
        </head>
        <body>
            <h1>Tunnlr error</h1>
            <p>An exception occurred while performing your request. If this is an error caused by Tunnlr, please <a href="https://github.com/gerwim/tunnlr/issues" target="_blank" rel="noopener">open an issue</a>.</p>
            <h2>Exception details</h2>
            <p>
                <code>{ex.Message}</code>
            </p>
            {RemainingErrors(ex.InnerException)}
        </body>
        </html>
        """"";

        string RemainingErrors(Exception? innerException)
        {
            var stringBuilder = new StringBuilder();
            while (innerException is not null)
            {
                stringBuilder.Append($"<p><code>{innerException.Message}</code></p>");
                innerException = innerException.InnerException;
            }
            return stringBuilder.ToString();
        }
    }
}