using Microsoft.EntityFrameworkCore;
using Tunnlr.Client.Persistence.Models;

namespace Tunnlr.Client.Persistence;

public class TunnlrClientDbContext : DbContext
{
    public TunnlrClientDbContext(DbContextOptions<TunnlrClientDbContext> options) : base(options)
    {
    }

    public DbSet<ReservedDomain> ReservedDomains { get; set; } = null!;
    public DbSet<Tunnel> Tunnels { get; set; } = null!;
}