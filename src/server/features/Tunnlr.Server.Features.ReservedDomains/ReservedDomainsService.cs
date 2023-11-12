using Tunnlr.Server.Core.Entities;
using Tunnlr.Server.Persistence;

namespace Tunnlr.Server.Features.ReservedDomains;

public class ReservedDomainsService : IReservedDomainsService
{
    private readonly TunnlrServerDbContext _tunnlrServerDbContext;

    public ReservedDomainsService(TunnlrServerDbContext tunnlrServerDbContext)
    {
        _tunnlrServerDbContext = tunnlrServerDbContext;
    }

    public ValueTask<ReservedDomain?> GetReservedDomain(string domain)
    {
        return _tunnlrServerDbContext.ReservedDomains.FindAsync(domain);
    }
}