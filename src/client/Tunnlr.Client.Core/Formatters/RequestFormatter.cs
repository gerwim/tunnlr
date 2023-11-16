using Tunnlr.Client.Core.Models;

namespace Tunnlr.Client.Core.Formatters;

public class RequestFormatter : IBaseRequestFormatter
{
    private readonly IEnumerable<IRequestFormatter> _requestFormatters;

    public RequestFormatter(IEnumerable<IRequestFormatter> requestFormatters)
    {
        _requestFormatters = requestFormatters;
    }

    public void Format(Request request)
    {
        foreach (var requestFormatter in _requestFormatters)
        {
            requestFormatter.Format(request);
        }
    }
}