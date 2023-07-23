using System.Collections.Concurrent;
using Grpc.Core;
using Tunnlr.Client.Core.Handlers;
using Tunnlr.Client.Core.Models;
using Tunnlr.Common.Protobuf;

namespace Tunnlr.Client.Core.Services;

public class TunnelService
{
    private readonly Tunnels.TunnelsClient _tunnelsClient;
    private readonly Requests.RequestsClient _requestsClient;

    public TunnelService(Tunnels.TunnelsClient tunnelsClient, Requests.RequestsClient requestsClient)
    {
        _tunnelsClient = tunnelsClient;
        _requestsClient = requestsClient;
    }

    public static ConcurrentDictionary<Guid, Tunnel> ActiveTunnels { get; } = new();

    public static event EventHandler<EventArgs>? ChangedEvent;
    private static void NotifyChanged(object sender, EventArgs e)
        => ChangedEvent?.Invoke(null, e);
    
    public Task<Tunnel> CreateTunnel(Tunnel tunnel)
    {
        var cancellationToken = new CancellationTokenSource();
        tunnel.CancellationTokenSource = cancellationToken;
        _ = Task.Run(async () =>
        {
            using var commandStream = _tunnelsClient.CreateTunnelStream(cancellationToken: cancellationToken.Token);

            await commandStream.RequestStream.WriteAsync(new CreateTunnelStreamRequest
            {
                StartTunnelRequest = new StartTunnelRequest
                {
                    TargetHost = tunnel.TargetUri
                }
            }, cancellationToken.Token);

            try
            {
                await foreach (var response in commandStream.ResponseStream.ReadAllAsync(
                                   cancellationToken: cancellationToken.Token))
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
                            var requestHandler = new RequestHandler(_requestsClient);
                            requestHandler.OpenRequestStream(tunnel, cancellationToken, response);
                            break;
                    }
                }
            }
            finally
            {
                ActiveTunnels.TryRemove(tunnel.TunnelId, out _);
                NotifyChanged(this, EventArgs.Empty);
            }
        }, cancellationToken.Token);

        ActiveTunnels.TryAdd(tunnel.TunnelId, tunnel);
        return Task.FromResult(tunnel);
    }

    public Task DestroyTunnel(Tunnel tunnel)
    {
        if (ActiveTunnels.TryGetValue(tunnel.TunnelId, out var activeTunnel))
        {
            activeTunnel.CancellationTokenSource?.Cancel();
        }

        return Task.CompletedTask;
    }
}