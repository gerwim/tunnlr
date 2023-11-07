using Google.Protobuf;
using Grpc.Core;
using Tunnlr.Client.Core.Models;
using Tunnlr.Client.Core.Services;
using Tunnlr.Common.Protobuf;
using Tunnlr.Common.Streams;

namespace Tunnlr.Client.Core.Handlers;

public class RequestHandler
{
    private readonly Requests.RequestsClient _requestsClient;

    public RequestHandler(Requests.RequestsClient requestsClient)
    {
        _requestsClient = requestsClient;
    }

    private BlockingStream BlockingStream { get; } = new();

    private Request? Request { get; set; }

    public void OpenRequestStream(Tunnel tunnel,
        CancellationTokenSource cancellationToken,
        CreateTunnelStreamResponse response)
    {
        var internalCancellationToken = new CancellationTokenSource();
        var linkedCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken.Token, internalCancellationToken.Token);
        Task.Run(async () =>
        {
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
                }, linkedCancellationToken.Token);


                await foreach (var requestStreamResponse in requestStream.ResponseStream.ReadAllAsync(
                                   cancellationToken: linkedCancellationToken.Token))
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
                            var task = Task.Run(
                                () => Http.InvokeRequest(result, BlockingStream),
                                cancellationToken.Token);

                            HandleOutgoingRequest(tunnel, internalCancellationToken, task, requestStream, Request);
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
        }, linkedCancellationToken.Token);
    }

    private static void HandleOutgoingRequest(Tunnel tunnel, CancellationTokenSource cancellationToken,
        Task<(HttpResponse? response, Stream? stream)> task,
        AsyncDuplexStreamingCall<ClientMessage, ServerMessage> requestStream, Request request)
    {
        Task.Run(async () =>
        {
            var httpResponse = await task;

            await requestStream.RequestStream.WriteAsync(new ClientMessage
            {
                HttpResponse = httpResponse.response
            }, cancellationToken.Token);

            if (httpResponse.response is null || httpResponse.stream is null) throw new Exception("Cancel");

            var response = new Response
            {
                HttpResponse = httpResponse.response
            };

            // Write the response stream back into the buffer and stream it back to the server
            int bytesRead;
            var buffer = new byte[64 * 1024];
            while ((bytesRead =
                       await httpResponse.stream.ReadAsync(buffer,
                           cancellationToken.Token)) > 0)
            {
                response.WriteToBody(buffer[..bytesRead]);
                await requestStream.RequestStream.WriteAsync(new ClientMessage
                {
                    ChunkedMessage = new ChunkedMessage
                    {
                        HttpRequestId = httpResponse.response.HttpRequestId,
                        Chunk = ByteString.CopyFrom(buffer[..bytesRead]),
                    }
                }, cancellationToken.Token);
            }

            await requestStream.RequestStream.WriteAsync(new ClientMessage
            {
                HttpRequestFinished = new HttpRequestFinished
                {
                    RequestId = httpResponse.response.HttpRequestId,
                }
            }, cancellationToken.Token);

            // Set response value to request
            if (TunnelService.ActiveTunnels.TryGetValue(tunnel.TunnelId, out var activeTunnel))
            {
                activeTunnel.Requests[request] = response;
                tunnel.NotifyChanged(null, EventArgs.Empty);
            }
            
            // End the tasks related to this request
            cancellationToken.Cancel();
        }, cancellationToken.Token);
    }
}