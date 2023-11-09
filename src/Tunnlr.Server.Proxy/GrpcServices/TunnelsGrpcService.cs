using System.Collections.Concurrent;
using Google.Protobuf;
using Grpc.Core;
using Tunnlr.Common.Exceptions;
using Tunnlr.Common.Protobuf;
using Tunnlr.Server.Core.Models;

namespace Tunnlr.Server.Proxy.GrpcServices;

public class TunnelsGrpcService : Tunnels.TunnelsBase
{
    private readonly ILogger<TunnelsGrpcService> _logger;
    private static readonly ConcurrentDictionary<string, Tunnel> ActiveTunnels = new();
    
    private readonly string _servedFromWildcard;
    private string? ServedFrom { get; set; }

    public TunnelsGrpcService(ILogger<TunnelsGrpcService> logger, IConfiguration configuration)
    {
        _logger = logger;
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
#if DEBUG
                        ServedFrom = _servedFromWildcard.Replace("*", "tunnel");
#else
                        ServedFrom = _servedFromWildcard.Replace("*", Guid.NewGuid().ToString());
#endif
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
                        break;
                }
            }
        }
        finally
        {
            if (ServedFrom is not null) ActiveTunnels.TryRemove(new Uri(ServedFrom).Host, out _);
        }
    }
    

    public Tunnel? GetTunnel(string servedHostname)
    {
        if (ActiveTunnels.TryGetValue(servedHostname, out var tunnel))
        {
            return tunnel;
        }

        return null;
    }
}