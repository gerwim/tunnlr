using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Tunnlr.Common.Protobuf;

namespace Tunnlr.Server.Core.Models;

public class StreamContext
{
    public HttpContext? HttpContext { get; set; }

    public IServerStreamWriter<ServerMessage>? GrpcStream { get; set; }
}