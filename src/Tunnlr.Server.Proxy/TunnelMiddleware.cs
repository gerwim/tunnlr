using Google.Protobuf;
using Tunnlr.Common;
using Tunnlr.Common.Protobuf;
using Tunnlr.Server.Core.Models;
using Tunnlr.Server.Proxy.GrpcServices;
using HttpRequest = Tunnlr.Common.Protobuf.HttpRequest;

namespace Tunnlr.Server.Proxy;

public class TunnelMiddleware
{
    private readonly RequestDelegate _nextMiddleware;

    public TunnelMiddleware(RequestDelegate nextMiddleware)
    {
        _nextMiddleware = nextMiddleware;
    }

    public async Task Invoke(HttpContext context, TunnelsGrpcService tunnelsGrpcService, RequestsGrpcService requestsGrpcService)
    {
        var tunnel = tunnelsGrpcService.GetTunnel(context.Request.Host.Host);
        if (tunnel is null) await _nextMiddleware.Invoke(context).ConfigureAwait(false);
        if (tunnel is not null)
        {
            var requestId = Guid.NewGuid();
            var request = new HttpRequest
            {
                HttpMethod = context.Request.Method.ConvertToHttpMethod(),
                Id = requestId.ToString(),
                TargetUri = $"{tunnel.Target}{context.Request.Path}{context.Request.QueryString}",
            };

            foreach (var httpRequestHeader in context.Request.Headers)
            {
                request.Headers.Add(httpRequestHeader.Key, httpRequestHeader.Value.First());
            }
            
            // Insert current context
            var streamContext = new StreamContext
            {
                HttpContext = context
            };
            tunnel.StreamContexts.TryAdd(requestId, streamContext);
            
            // Open up a new request stream for this request
            if (tunnel.CommandStream is null) throw new Exception("Cancel");
            await tunnel.CommandStream.WriteAsync(new CreateTunnelStreamResponse
            {
                OpenRequestStream = new OpenRequestStream
                {
                    RequestId = requestId.ToString(),
                    ServedFrom = context.Request.Host.Host,
                }
            }).ConfigureAwait(false);
            // Wait for requestStream to be opened
            while (streamContext.GrpcStream is null)
            {
                await Task.Delay(25).ConfigureAwait(false);
            }
            // Write headers
            await streamContext.GrpcStream.WriteAsync(new ServerMessage
            {
                HttpRequest = request
            }).ConfigureAwait(false);
            
            // Write data
            int bytesRead;
            var buffer = new byte[64 * 1024];
            while ((bytesRead = await context.Request.Body.ReadAsync(buffer).ConfigureAwait(false)) > 0)
            {
                await streamContext.GrpcStream.WriteAsync(new ServerMessage
                {
                    ChunkedMessage = new ChunkedMessage
                    {
                        HttpRequestId = requestId.ToString(),
                        Chunk = ByteString.CopyFrom(buffer[..bytesRead]),
                    }
                }).ConfigureAwait(false);
            }
            // Send finished marker
            await streamContext.GrpcStream.WriteAsync(new ServerMessage
            {
                HttpRequestFinished = new HttpRequestFinished
                {
                    RequestId = requestId.ToString(),
                }
            }).ConfigureAwait(false);

            // Wait for response from stream
            await requestsGrpcService.WaitForCompletion(requestId).ConfigureAwait(false);
        }
    }
}