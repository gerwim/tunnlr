using AsyncAwaitBestPractices;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Tunnlr.Client.Core.Contracts.Exceptions;
using Tunnlr.Client.Core.Formatters;
using Tunnlr.Client.Core.Models;
using Tunnlr.Client.Core.Services;
using Tunnlr.Client.Features.Interceptors;
using Tunnlr.Client.Features.Interceptors.Core;
using Tunnlr.Common.Protobuf;
using Tunnlr.Common.Streams;

namespace Tunnlr.Client.Core.Handlers;

public class RequestHandler
{
    private readonly Requests.RequestsClient _requestsClient;
    private readonly IBaseRequestFormatter _requestFormatter;
    private readonly IBaseResponseFormatter _responseFormatter;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RequestHandler> _logger;

    public RequestHandler(Requests.RequestsClient requestsClient, ILogger<RequestHandler> logger, IServiceProvider serviceProvider, IBaseRequestFormatter requestFormatter, IBaseResponseFormatter responseFormatter)
    {
        _requestsClient = requestsClient;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _requestFormatter = requestFormatter;
        _responseFormatter = responseFormatter;
    }

    private BlockingStream BlockingStream { get; } = new();

    private Request? Request { get; set; }

    public async Task OpenRequestStream(Tunnel tunnel,
        CancellationToken cancellationToken,
        CreateTunnelStreamResponse response)
    {
        var internalCts = new CancellationTokenSource();
        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, internalCts.Token);
        
        try
        {
            using var requestStream =
                _requestsClient.CreateRequestStream(cancellationToken: linkedCts.Token);

            await requestStream.RequestStream.WriteAsync(new ClientMessage
            {
                RequestStreamCreated = new RequestStreamCreated
                {
                    RequestId = response.OpenRequestStream.RequestId,
                    SecurityKey = ByteString.CopyFrom(tunnel.SecurityKey),
                    ServedFrom = response.OpenRequestStream.ServedFrom,
                }
            }, linkedCts.Token).ConfigureAwait(false);
            
            await foreach (var requestStreamResponse in requestStream.ResponseStream.ReadAllAsync(
                               cancellationToken: linkedCts.Token).ConfigureAwait(false))
            {
                switch (requestStreamResponse.DataCase)
                {
                    case ServerMessage.DataOneofCase.HttpRequest:
                    {
                        var result = requestStreamResponse.HttpRequest;
                        Request = new Request(result);
                        
                        // Execute request interceptors
                        foreach (var tunnelRequestInterceptor in tunnel.RequestInterceptors.Where(x => x.Enabled))
                        {
                            var type = Type.GetType(tunnelRequestInterceptor.TypeName);
                            var interceptor =  (IRequestInterceptor) InterceptorFactory.FromType(_serviceProvider, type!, tunnelRequestInterceptor.Values);
                            await interceptor.InvokeAsync(Request).ConfigureAwait(false);
                        }
                        
                        tunnel.Requests.TryAdd(Request, null);
                       
                        tunnel.NotifyChanged(this, EventArgs.Empty);
                        var httpRequestResult = Http.InvokeRequest(result, BlockingStream, linkedCts.Token, _serviceProvider);

                        HandleOutgoingRequest(tunnel, linkedCts.Token, httpRequestResult, requestStream)
                            .SafeFireAndForget(ex => _logger.LogError(ex, "Error handling outgoing request"));
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
                        // Format request
                        _requestFormatter.Format(Request!);
                        tunnel.NotifyChanged(this, EventArgs.Empty);
                        break;
                    }
                    case ServerMessage.DataOneofCase.CloseStream:
                        internalCts.Cancel();
                        break;
                }
            }
        }
        catch (RpcException e) when (e.Status.StatusCode == StatusCode.Cancelled)
        {
            // Ignore cancellation
        }
    }

    private async Task HandleOutgoingRequest(Tunnel tunnel, CancellationToken cancellationToken,
        Task<HttpInvokeRequestResult> httpTask,
        AsyncDuplexStreamingCall<ClientMessage, ServerMessage> requestStream)
    {
        var httpResponse = await httpTask.ConfigureAwait(false);

        if (httpResponse.Response is null || httpResponse.Stream is null) throw new OutgoingRequestException("Cancel outgoing request: there is neither a response or a stream");
        var response = new Response(httpResponse.Response);
        
        // Execute response interceptors
        foreach (var tunnelResponseInterceptor in tunnel.ResponseInterceptors.Where(x => x.Enabled))
        {
            var type = Type.GetType(tunnelResponseInterceptor.TypeName);
            // TODO: check for nullability
            var interceptor =  (IResponseInterceptor) InterceptorFactory.FromType(_serviceProvider, type!, tunnelResponseInterceptor.Values);
            await interceptor.InvokeAsync(response).ConfigureAwait(false);
        }
        
        await requestStream.RequestStream.WriteAsync(new ClientMessage
        {
            HttpResponse = response.HttpResponse
        }, cancellationToken).ConfigureAwait(false);
        
        // Write the response stream back into the buffer and stream it back to the server
        int bytesRead;
        var buffer = new byte[64 * 1024];
        while ((bytesRead =
                   await httpResponse.Stream.ReadAsync(buffer,
                       cancellationToken).ConfigureAwait(false)) > 0)
        {
            response.WriteToBody(buffer[..bytesRead]);
            await requestStream.RequestStream.WriteAsync(new ClientMessage
            {
                ChunkedMessage = new ChunkedMessage
                {
                    HttpRequestId = httpResponse.Response.HttpRequestId,
                    Chunk = ByteString.CopyFrom(buffer[..bytesRead]),
                }
            }, cancellationToken).ConfigureAwait(false);
        }
        
        // Set response value to request
        if (TunnelService.ActiveTunnels.TryGetValue(tunnel.TunnelId, out var activeTunnel))
        {
            activeTunnel.Requests[Request!] = response;
            // Format response
            _responseFormatter.Format(response);
            
            tunnel.NotifyChanged(null, EventArgs.Empty);
        }

        await requestStream.RequestStream.WriteAsync(new ClientMessage
        {
            HttpRequestFinished = new HttpRequestFinished
            {
                RequestId = httpResponse.Response.HttpRequestId,
            }
        }, cancellationToken).ConfigureAwait(false);
    }
}