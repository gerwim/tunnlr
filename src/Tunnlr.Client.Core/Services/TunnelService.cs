using System.Collections.Concurrent;
using AsyncAwaitBestPractices;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Tunnlr.Client.Core.Handlers;
using Tunnlr.Client.Core.Models;
using Tunnlr.Common.Protobuf;

namespace Tunnlr.Client.Core.Services;

public class TunnelService
{
    private readonly Tunnels.TunnelsClient _tunnelsClient;
    private readonly Requests.RequestsClient _requestsClient;
    private readonly ILogger<TunnelService> _logger;

    public TunnelService(Tunnels.TunnelsClient tunnelsClient, Requests.RequestsClient requestsClient, ILogger<TunnelService> logger)
    {
        _tunnelsClient = tunnelsClient;
        _requestsClient = requestsClient;
        _logger = logger;
    }

    public static ConcurrentDictionary<Guid, Tunnel> ActiveTunnels { get; } = new();

    public static event EventHandler<EventArgs>? ChangedEvent;
    private static void NotifyChanged(object sender, EventArgs e)
        => ChangedEvent?.Invoke(null, e);
    
    public Tunnel CreateTunnel(Tunnel tunnel)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        tunnel.CancellationTokenSource = cancellationTokenSource;
       
        CreateTunnel(tunnel, cancellationTokenSource.Token).SafeFireAndForget(ex => _logger.LogError("Error: {Exception}", ex));

        ActiveTunnels.TryAdd(tunnel.TunnelId, tunnel);
        return tunnel;
    }
    
    public void DestroyTunnel(Tunnel tunnel)
    {
        if (ActiveTunnels.TryGetValue(tunnel.TunnelId, out var activeTunnel))
        {
            activeTunnel.CancellationTokenSource?.Cancel();
        }
    }
    
    private async Task CreateTunnel(Tunnel tunnel, CancellationToken cancellationToken)
    {
        try
        {
            using var commandStream = _tunnelsClient.CreateTunnelStream(cancellationToken:cancellationToken);

            await commandStream.RequestStream.WriteAsync(new CreateTunnelStreamRequest
            {
                StartTunnelRequest = new StartTunnelRequest
                {
                    TargetHost = tunnel.TargetUri
                }
            }, cancellationToken).ConfigureAwait(false);

        
            await foreach (var response in commandStream.ResponseStream.ReadAllAsync(
                               cancellationToken: cancellationToken).ConfigureAwait(false))
            {
                switch (response.DataCase)
                {
                    case CreateTunnelStreamResponse.DataOneofCase.StartTunnelResponse:
                        if (ActiveTunnels.TryGetValue(tunnel.TunnelId, out var activeTunnel))
                        {
                            activeTunnel.Status = TunnelStatus.Started;
                            activeTunnel.ServedFrom = response.StartTunnelResponse.ServedFrom;
                            activeTunnel.SecurityKey = response.StartTunnelResponse.SecurityKey.ToByteArray();
                        }

                        NotifyChanged(this, EventArgs.Empty);
                        break;
                    case CreateTunnelStreamResponse.DataOneofCase.OpenRequestStream:
                        var requestHandler = new RequestHandler(_requestsClient, _logger);
                        requestHandler.OpenRequestStream(tunnel, cancellationToken, response).SafeFireAndForget(ex => _logger.LogError("Error {Exception}", ex.Message));
                        break;
                }
            }
        }
        catch (RpcException e) when (e.Status.StatusCode == StatusCode.Cancelled)
        {
            // Ignore cancellation
        }
        finally
        {
            ActiveTunnels.TryRemove(tunnel.TunnelId, out _);
            NotifyChanged(this, EventArgs.Empty);
        }
    }
}