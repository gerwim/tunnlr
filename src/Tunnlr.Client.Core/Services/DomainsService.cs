using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tunnlr.Client.Core.Models;
using Tunnlr.Client.Persistence;
using Tunnlr.Common.Protobuf;

namespace Tunnlr.Client.Core.Services;

public class DomainsService
{
    private readonly Domains.DomainsClient _domainsClient;
    private readonly TunnlrDbContext _tunnlrDbContext;
    private readonly ILogger<DomainsService> _logger;

    public DomainsService(Domains.DomainsClient domainsClient, ILogger<DomainsService> logger, TunnlrDbContext tunnlrDbContext)
    {
        _domainsClient = domainsClient;
        _logger = logger;
        _tunnlrDbContext = tunnlrDbContext;
    }

    public async Task<string> ReserveDomain(string prefix)
    {
        var result = await _domainsClient.CreateReservedDomainAsync(new CreateReservedDomainRequest
        {
            DomainPrefix = prefix
        });

        return result.Domain;
    }

    public async Task<IEnumerable<ReservedDomain>> GetReservedDomains()
    {
        var result = await _domainsClient.GetReservedDomainsAsync(new Empty());
        var domains = result.Domains.Select(x => new ReservedDomain { Domain = x }).ToList();

        await SyncDomains(domains.Select(x => x.Domain).ToList()).ConfigureAwait(false);
        return domains;
    }
    
    public async Task DeleteReservedDomain(string domain)
    {
        await _domainsClient.DeleteReservedDomainAsync(new DeleteReservedDomainRequest
        {
            Domain = domain
        });
    }

    /// <summary>
    /// Syncs remote domains with the local database
    /// </summary>
    private async Task SyncDomains(List<string> remoteDomains)
    {
        var localDomains = _tunnlrDbContext.ReservedDomains.AsTracking().ToList();

        var insert = remoteDomains.Where(x => localDomains.All(l => l.Domain != x));
        var delete = localDomains.Where(x => !remoteDomains.Contains(x.Domain));
        
        foreach (var domain in insert)
        {
            _tunnlrDbContext.Add(new Persistence.Models.ReservedDomain
            {
                Domain = domain
            });
        }
        foreach (var domain in delete)
        {
            _tunnlrDbContext.Remove(domain);
        }

        await _tunnlrDbContext.SaveChangesAsync().ConfigureAwait(false);
    }
}