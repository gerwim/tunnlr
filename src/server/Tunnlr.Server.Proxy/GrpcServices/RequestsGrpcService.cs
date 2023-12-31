﻿using System.Collections.Concurrent;
using Grpc.Core;
using Tunnlr.Common.Protobuf;
using Tunnlr.Server.Core.Models;
using HttpResponse = Tunnlr.Common.Protobuf.HttpResponse;

namespace Tunnlr.Server.Proxy.GrpcServices;

public class RequestsGrpcService : Requests.RequestsBase
{
    private readonly ILogger<RequestsGrpcService> _logger;
    private readonly TunnelsGrpcService _tunnelsGrpcService;
    private static readonly ConcurrentDictionary<Guid, byte> FinishedResponses = new();

    private Tunnel? Tunnel { get; set; }
    private Guid RequestId { get; set; }

    public RequestsGrpcService(ILogger<RequestsGrpcService> logger, TunnelsGrpcService tunnelsGrpcService)
    {
        _logger = logger;
        _tunnelsGrpcService = tunnelsGrpcService;
    }

    public override async Task CreateRequestStream(IAsyncStreamReader<ClientMessage> requestStream, IServerStreamWriter<ServerMessage> responseStream,
        ServerCallContext context)
    {
        try
        {
            await foreach (var request in requestStream.ReadAllAsync().ConfigureAwait(false))
            {
                await HandleRequest(request, responseStream).ConfigureAwait(false);
            }
        }
        finally
        {
            if (Tunnel is not null) Tunnel.StreamContexts.TryRemove(RequestId, out _);
        }
    }

    public void SetResponseFinished(Guid requestId)
    {
        FinishedResponses.TryAdd(requestId, 0);
    }

    public async Task WaitForCompletion(Guid requestId, CancellationToken cancellationToken = default)
    {
        while (!FinishedResponses.TryGetValue(requestId, out _))
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            await Task.Delay(25, cancellationToken).ConfigureAwait(false);
        }
        
        FinishedResponses.TryRemove(requestId, out _);
    }

    /// <summary>
    /// Handle all stream requests
    /// </summary>
    /// <param name="request"></param>
    /// <param name="responseStream"></param>
    private async Task HandleRequest(ClientMessage request,
        IServerStreamWriter<ServerMessage> responseStream)
    {
        switch (request.DataCase)
        {
            case ClientMessage.DataOneofCase.RequestStreamCreated:
            {
                var result = request.RequestStreamCreated;
                
                Tunnel = _tunnelsGrpcService.GetTunnel(result.ServedFrom);
                if (Tunnel is null) throw new Exception("Cancel"); // TODO: should return proper stream error in web instead of loading forever

                var securityKey = Tunnel.SecurityKey;
                if (!securityKey.SequenceEqual(result.SecurityKey.ToByteArray())) throw new Exception("Cancel");
                RequestId = Guid.Parse(result.RequestId);

                Tunnel.StreamContexts.TryGetValue(RequestId, out var streamContext);

                if (streamContext is null) throw new Exception("Cancel");
                streamContext.GrpcStream = responseStream; // TODO: handle nullability
                break;
            }
            case ClientMessage.DataOneofCase.HttpResponse:
            {
                var result = request.HttpResponse;
                if (Tunnel is null) throw new Exception("Cancel");

                Tunnel.StreamContexts.TryGetValue(RequestId, out var streamContext);
                if (streamContext?.HttpContext is null) throw new Exception("Cancel");
                SetHeaders(streamContext.HttpContext, result, Tunnel.Target);
                streamContext.HttpContext.Response.StatusCode = result.StatusCode;
                break;
            }
            case ClientMessage.DataOneofCase.ChunkedMessage:
            {
                var result = request.ChunkedMessage;
                if (Tunnel is null) throw new Exception("Cancel");
                
                Tunnel.StreamContexts.TryGetValue(RequestId, out var streamContext);

                if (streamContext?.HttpContext is null) throw new Exception("Cancel");
                await streamContext.HttpContext.Response.Body.WriteAsync(result.Chunk.ToByteArray()).ConfigureAwait(false);
                await streamContext.HttpContext.Response.Body.FlushAsync().ConfigureAwait(false);
                break;
            }
            case ClientMessage.DataOneofCase.HttpRequestFinished:
            {
                var result = request.HttpRequestFinished;
                
                SetResponseFinished(Guid.Parse(result.RequestId));
                break;
            }
        }
    }
    
    private void SetHeaders(HttpContext context, HttpResponse clientResponse, string targetUri)
    {
        var host = context.Request.Host;
        var targetHost = new Uri(targetUri).Host;
        context.Response.Headers.Clear();
        foreach (var header in clientResponse.Headers)
        {
            var val = header.Value;
            if (header.Key.ToLowerInvariant() == "transfer-encoding") continue;
            if (header.Key.ToLowerInvariant() == "location") val = header.Value.Replace(targetHost, host.ToString());
            context.Response.Headers.TryAdd(header.Key, val);
        }
    }
}