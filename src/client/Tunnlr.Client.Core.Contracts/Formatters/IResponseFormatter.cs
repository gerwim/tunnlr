using Tunnlr.Client.Core.Models;

namespace Tunnlr.Client.Core.Contracts.Formatters;

public interface IResponseFormatter
{
    public void Format(IResponse response);
}