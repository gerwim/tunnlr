using Tunnlr.Server.Core.Entities;
using Tunnlr.Server.Persistence;

namespace Tunnlr.Server.Features.ReservedDomains;

public class ReservedDomainsService : IReservedDomainsService
{
    private readonly TunnlrDbContext _tunnlrDbContext;

    public ReservedDomainsService(TunnlrDbContext tunnlrDbContext)
    {
        _tunnlrDbContext = tunnlrDbContext;
    }

    public ValueTask<ReservedDomain?> GetReservedDomain(string domain)
    {
        return _tunnlrDbContext.ReservedDomains.FindAsync(domain);
    }
}