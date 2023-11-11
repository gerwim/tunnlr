using Microsoft.EntityFrameworkCore;
using Tunnlr.Server.Core.Entities;

namespace Tunnlr.Server.Persistence;

public abstract class TunnlrDbContext : DbContext
{
    protected TunnlrDbContext(DbContextOptions options) : base(options)
    {
    }
    
    public DbSet<ReservedDomain> ReservedDomains { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
}