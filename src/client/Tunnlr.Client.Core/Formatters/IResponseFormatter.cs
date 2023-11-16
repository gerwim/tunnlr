using Tunnlr.Client.Core.Models;

namespace Tunnlr.Client.Core.Formatters;

public interface IResponseFormatter
{
    public void Format(Response response);
}