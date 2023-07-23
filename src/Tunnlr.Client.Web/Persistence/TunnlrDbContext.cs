using Microsoft.EntityFrameworkCore;
using Tunnlr.Client.Web.Persistence.Models;

namespace Tunnlr.Client.Web.Persistence;

public class TunnlrDbContext : DbContext
{
    public TunnlrDbContext(DbContextOptions<TunnlrDbContext> options) : base(options)
    {
    }

    public DbSet<Tunnel> Tunnels { get; set; } = null!;
}