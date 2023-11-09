using AsyncAwaitBestPractices;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Tunnlr.Client.Core.Models;
using Tunnlr.Client.Core.Services;
using Tunnlr.Common.Protobuf;
using Tunnlr.Common.Streams;

namespace Tunnlr.Client.Core.Handlers;

public class RequestHandler
{
    private readonly Requests.RequestsClient _requestsClient;
    private readonly ILogger _logger;

    public RequestHandler(Requests.RequestsClient requestsClient, ILogger logger)
    {
        _requestsClient = requestsClient;
        _logger = logger;
    }

    private BlockingStream BlockingStream { get; } = new();

    private Request? Request { get; set; }

    public async Task OpenRequestStream(Tunnel tunnel,
        CancellationToken cancellationToken,
        CreateTunnelStreamResponse response)
    {
        var internalCancellationToken = new CancellationTokenSource();
        var linkedCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, internalCancellationToken.Token);
        
        try
        {
            using var requestStream =
                _requestsClient.CreateRequestStream(cancellationToken: linkedCancellationToken.Token);

            await requestStream.RequestStream.WriteAsync(new ClientMessage
            {
                RequestStreamCreated = new RequestStreamCreated
                {
                    RequestId = response.OpenRequestStream.RequestId,
                    SecurityKey = ByteString.CopyFrom(tunnel.SecurityKey),
                    ServedFrom = response.OpenRequestStream.ServedFrom,
                }
            }, linkedCancellationToken.Token).ConfigureAwait(false);


            await foreach (var requestStreamResponse in requestStream.ResponseStream.ReadAllAsync(
                               cancellationToken: linkedCancellationToken.Token).ConfigureAwait(false))
            {
                switch (requestStreamResponse.DataCase)
                {
                    case ServerMessage.DataOneofCase.HttpRequest:
                    {
                        var result = requestStreamResponse.HttpRequest;

                        Request = new Request
                        {
                            HttpRequest = result,
                        };
                        tunnel.Requests.TryAdd(Request, null);
                        tunnel.NotifyChanged(this, EventArgs.Empty);
                        var httpRequestResult = Http.InvokeRequest(result, BlockingStream, linkedCancellationToken.Token);

                        HandleOutgoingRequest(tunnel, internalCancellationToken, httpRequestResult, requestStream, Request).SafeFireAndForget(ex => _logger.LogError("Error: {Exception}", ex.Message));
                        break;
                    }
                    case ServerMessage.DataOneofCase.ChunkedMessage:
                    {
                        var result = requestStreamResponse.ChunkedMessage;

                        var bytes = result.Chunk.ToByteArray();
                        Request?.WriteToBody(bytes);
                        BlockingStream.Write(bytes);
                        break;
                    }
                    case ServerMessage.DataOneofCase.HttpRequestFinished:
                    {
                        BlockingStream.SetEndOfStream();
                        tunnel.NotifyChanged(this, EventArgs.Empty);
                        break;
                    }
                }
            }
        }
        catch (RpcException e) when (e.Status.StatusCode == StatusCode.Cancelled)
        {
            // Ignore cancellation
        }
    }

    private static async Task HandleOutgoingRequest(Tunnel tunnel, CancellationTokenSource cancellationToken,
        Task<HttpInvokeRequestResult> httpTask,
        AsyncDuplexStreamingCall<ClientMessage, ServerMessage> requestStream, Request request)
    {
        var httpResponse = await httpTask.ConfigureAwait(false);
        
        await requestStream.RequestStream.WriteAsync(new ClientMessage
        {
            HttpResponse = httpResponse.Response
        }, cancellationToken.Token).ConfigureAwait(false);

        if (httpResponse.Response is null || httpResponse.Stream is null) throw new Exception("Cancel");

        var response = new Response
        {
            HttpResponse = httpResponse.Response
        };

        // Write the response stream back into the buffer and stream it back to the server
        int bytesRead;
        var buffer = new byte[64 * 1024];
        while ((bytesRead =
                   await httpResponse.Stream.ReadAsync(buffer,
                       cancellationToken.Token).ConfigureAwait(false)) > 0)
        {
            response.WriteToBody(buffer[..bytesRead]);
            await requestStream.RequestStream.WriteAsync(new ClientMessage
            {
                ChunkedMessage = new ChunkedMessage
                {
                    HttpRequestId = httpResponse.Response.HttpRequestId,
                    Chunk = ByteString.CopyFrom(buffer[..bytesRead]),
                }
            }, cancellationToken.Token).ConfigureAwait(false);
        }

        await requestStream.RequestStream.WriteAsync(new ClientMessage
        {
            HttpRequestFinished = new HttpRequestFinished
            {
                RequestId = httpResponse.Response.HttpRequestId,
            }
        }, cancellationToken.Token).ConfigureAwait(false);

        // Set response value to request
        if (TunnelService.ActiveTunnels.TryGetValue(tunnel.TunnelId, out var activeTunnel))
        {
            activeTunnel.Requests[request] = response;
            tunnel.NotifyChanged(null, EventArgs.Empty);
        }
            
        // End the tasks related to this request
        cancellationToken.Cancel();
    }
}