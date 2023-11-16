using System.Collections.Concurrent;
using AsyncAwaitBestPractices;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tunnlr.Client.Core.Exceptions;
using Tunnlr.Client.Core.Handlers;
using Tunnlr.Client.Core.Models;
using Tunnlr.Client.Persistence;
using Tunnlr.Common.Protobuf;

namespace Tunnlr.Client.Core.Services;

public class TunnelService
{
    private readonly Tunnels.TunnelsClient _tunnelsClient;
    private readonly Requests.RequestsClient _requestsClient;
    private readonly ILogger<TunnelService> _logger;
    private readonly TunnlrClientDbContext _tunnlrClientDbContext;
    private readonly IServiceProvider _serviceProvider;

    public TunnelService(Tunnels.TunnelsClient tunnelsClient, Requests.RequestsClient requestsClient, ILogger<TunnelService> logger, TunnlrClientDbContext tunnlrClientDbContext, IServiceProvider serviceProvider)
    {
        _tunnelsClient = tunnelsClient;
        _requestsClient = requestsClient;
        _logger = logger;
        _tunnlrClientDbContext = tunnlrClientDbContext;
        _serviceProvider = serviceProvider;
    }

    public static ConcurrentDictionary<Guid, Tunnel> ActiveTunnels { get; } = new();

    public static event EventHandler<EventArgs>? ChangedEvent;
    private static void NotifyChanged(object sender, EventArgs e)
        => ChangedEvent?.Invoke(null, e);
    
    public Tunnel CreateTunnel(Tunnel tunnel)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        tunnel.CancellationTokenSource = cancellationTokenSource;
       
        CreateTunnel(tunnel, cancellationTokenSource.Token).SafeFireAndForget(ex => _logger.LogError(ex, "Error creating tunnel"));

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

    public async Task AddTunnel(string target)
    {
        string? targetHost;
        try
        {
            targetHost = ConvertToValidTarget(target);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not convert target uri");
            throw new InvalidTunnelTargetException("You've entered an invalid target.", ex);
        }
        
        await _tunnlrClientDbContext.AddAsync(new Tunnlr.Client.Persistence.Models.Tunnel
        {
            TargetUri = targetHost
        }).ConfigureAwait(false);

        await _tunnlrClientDbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    private string ConvertToValidTarget(string target)
    {
        var uri = new Uri(target);
        return uri.GetLeftPart(UriPartial.Authority);
    }
    
    private async Task CreateTunnel(Tunnel tunnel, CancellationToken cancellationToken)
    {
        try
        {
            using var commandStream = _tunnelsClient.CreateTunnelStream(cancellationToken: cancellationToken);

            await commandStream.RequestStream.WriteAsync(new CreateTunnelStreamRequest
            {
                StartTunnelRequest = new StartTunnelRequest
                {
                    TargetHost = tunnel.TargetUri,
                    UseReservedDomain = tunnel.ReservedDomain?.Domain,
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
                        var requestHandler = ActivatorUtilities.CreateInstance<RequestHandler>(_serviceProvider);
                        requestHandler.OpenRequestStream(tunnel, cancellationToken, response).SafeFireAndForget(ex => _logger.LogError(ex, "Error opening request stream"));
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