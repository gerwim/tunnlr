using System.Text;
using Tunnlr.Client.Core.Formatters;
using Tunnlr.Client.Core.Models;
using Tunnlr.Client.Features.Formatters.Extensions;

namespace Tunnlr.Client.Features.Formatters;

public class JsonFormatter : IRequestFormatter
{
    public bool IsJsonRequest(Request request)
    {
        request.HttpRequest.Headers.TryGetValue("Content-Type", out var header);

        return header == "application/json";
    }
    public void Format(Request request)
    {
        if (!IsJsonRequest(request)) return;
        
        var partialSubTitle = Encoding.UTF8.GetString(request.Body.Take(40).ToArray());
        request.SubTitle = $"{(partialSubTitle.Length < request.Body.Count ? $"{partialSubTitle}..." : partialSubTitle)}".ToCleanString();
    }
}