using Tunnlr.Client.Core.Models;

namespace Tunnlr.Client.Core.Contracts.Formatters;

public interface IBaseResponseFormatter
{
    public void Format(IResponse response);
}