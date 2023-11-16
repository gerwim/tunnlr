using Tunnlr.Client.Core.Models;

namespace Tunnlr.Client.Core.Formatters;

public class ResponseFormatter : IBaseResponseFormatter
{
    private readonly IEnumerable<IResponseFormatter> _requestFormatters;

    public ResponseFormatter(IEnumerable<IResponseFormatter> requestFormatters)
    {
        _requestFormatters = requestFormatters;
    }

    public void Format(Response response)
    {
        foreach (var requestFormatter in _requestFormatters)
        {
            requestFormatter.Format(response);
        }
    }
}