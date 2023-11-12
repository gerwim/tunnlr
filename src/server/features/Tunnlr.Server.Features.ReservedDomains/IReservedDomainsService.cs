using Tunnlr.Server.Core.Entities;

namespace Tunnlr.Server.Features.ReservedDomains;

public interface IReservedDomainsService
{
    ValueTask<ReservedDomain?> GetReservedDomain(string domain);
}