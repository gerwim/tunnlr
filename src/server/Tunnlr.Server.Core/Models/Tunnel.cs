using System.Collections.Concurrent;
using Grpc.Core;
using Tunnlr.Common.Protobuf;
using Tunnlr.Server.Core.Helpers;

namespace Tunnlr.Server.Core.Models;

public class Tunnel
{
    public Guid Id { get; set; }
    public required string ServedFrom { get; set; }
    
    public required string Target { get; set; }

    public byte[] SecurityKey { get; } = Security.GenerateSecureKey();
    
    public required IServerStreamWriter<CreateTunnelStreamResponse> CommandStream { get; init; }
    
    public ConcurrentDictionary<Guid, StreamContext> StreamContexts { get; } = new();
}