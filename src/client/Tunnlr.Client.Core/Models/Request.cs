using Tunnlr.Common.Protobuf;

namespace Tunnlr.Client.Core.Models;

public class Request : RequestBase
{
    public required HttpRequest HttpRequest { get; set; }
}