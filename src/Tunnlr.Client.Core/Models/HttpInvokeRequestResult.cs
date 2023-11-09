using HttpResponse = Tunnlr.Common.Protobuf.HttpResponse;

namespace Tunnlr.Client.Core.Models;

public class HttpInvokeRequestResult
{
    public HttpResponse? Response { get; set; }
    public Stream? Stream { get; set; }
}