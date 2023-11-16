using Tunnlr.Client.Core.Models;

namespace Tunnlr.Client.Core.Formatters;

public interface IBaseResponseFormatter
{
    public void Format(Response response);
}