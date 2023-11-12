using System.Collections.Concurrent;
using Google.Protobuf;
using Grpc.Core;
using Tunnlr.Common.Exceptions;
using Tunnlr.Common.Protobuf;
using Tunnlr.Server.Core.Models;
using Tunnlr.Server.Features.ReservedDomains;

namespace Tunnlr.Server.Proxy.GrpcServices;

public class TunnelsGrpcService : Tunnels.TunnelsBase
{
    private readonly ILogger<TunnelsGrpcService> _logger;
    private readonly IReservedDomainsService _reservedDomainService;
    private static readonly ConcurrentDictionary<string, Tunnel> ActiveTunnels = new();
    
    private readonly string _servedFromWildcard;
    private string? ServedFrom { get; set; }

    public TunnelsGrpcService(ILogger<TunnelsGrpcService> logger, IConfiguration configuration, IReservedDomainsService reservedDomainService)
    {
        _logger = logger;
        _reservedDomainService = reservedDomainService;
        _servedFromWildcard = configuration.GetRequiredSection("Tunnlr:Server:Proxy")
            .GetValue<string>("ServedFromWildcard") ?? throw new InvalidConfigurationException("Option value Tunnlr:Server:Proxy:ServedFromWildcard is missing");
    }

    public override async Task CreateTunnelStream(IAsyncStreamReader<CreateTunnelStreamRequest> requestStream, IServerStreamWriter<CreateTunnelStreamResponse> responseStream,
        ServerCallContext context)
    {
        try
        {
            await foreach (var request in requestStream.ReadAllAsync(context.CancellationToken).ConfigureAwait(false))
            {
                switch (request.DataCase)
                {
                    case CreateTunnelStreamRequest.DataOneofCase.StartTunnelRequest:
                        await StartTunnel(request, responseStream, context).ConfigureAwait(false);
                        break;
                }
            }
        }
        finally
        {
            if (ServedFrom is not null) ActiveTunnels.TryRemove(new Uri(ServedFrom).Host, out _);
        }
    }


    private async Task StartTunnel(CreateTunnelStreamRequest request,  IServerStreamWriter<CreateTunnelStreamResponse> responseStream, ServerCallContext context)
    {
        string servedFromDomain = Guid.NewGuid().ToString();
                        
        if (!string.IsNullOrWhiteSpace(request.StartTunnelRequest.UseReservedDomain))
        {
            var userId = context.GetHttpContext().User.Identity?.Name;
            if (userId is null) throw new RpcException(new Status(StatusCode.Unauthenticated, "User is unauthenticated"));

            var reservedDomain = await _reservedDomainService.GetReservedDomain(request.StartTunnelRequest.UseReservedDomain).ConfigureAwait(false);

            if (reservedDomain?.UserId == userId && !ActiveTunnels.TryGetValue(new Uri(BuildServedFromName(reservedDomain.Domain)).Host, out _))
            {
                servedFromDomain = reservedDomain.Domain;
            }
        }

        ServedFrom = BuildServedFromName(servedFromDomain);

        var tunnel = new Tunnel
        {
            ServedFrom = ServedFrom,
            Target = request.StartTunnelRequest.TargetHost,
            CommandStream = responseStream
        };

        ActiveTunnels.TryAdd(new Uri(ServedFrom).Host, tunnel);
                        
        await responseStream.WriteAsync(new CreateTunnelStreamResponse
        {
            StartTunnelResponse = new StartTunnelResponse
            {
                ServedFrom = tunnel.ServedFrom,
                SecurityKey = ByteString.CopyFrom(tunnel.SecurityKey), 
            }
        }).ConfigureAwait(false);
    }
    
    public Tunnel? GetTunnel(string servedHostname)
    {
        if (ActiveTunnels.TryGetValue(servedHostname, out var tunnel))
        {
            return tunnel;
        }

        return null;
    }

    private string BuildServedFromName(string domain)
    {
        return _servedFromWildcard.Replace("*", domain);
    }
}