using Tunnlr.Client.Core.Models;

namespace Tunnlr.Client.Core.Formatters;

public interface IBaseRequestFormatter
{
    public void Format(Request request);
}