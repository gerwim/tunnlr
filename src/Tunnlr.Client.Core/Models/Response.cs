using Tunnlr.Common.Protobuf;

namespace Tunnlr.Client.Core.Models;

public class Response : RequestBase
{
    public required HttpResponse HttpResponse { get; set; }
}