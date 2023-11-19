using Tunnlr.Client.Core.Contracts.Models;

namespace Tunnlr.Client.Core.Contracts.Formatters;

public interface IBaseRequestFormatter
{
    public void Format(IRequest request);
}