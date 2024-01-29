using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;
using Tunnlr.Client.Persistence.Models;

namespace Tunnlr.Client.Persistence;

public class TunnlrClientDbContext : DbContext
{
    public TunnlrClientDbContext(DbContextOptions<TunnlrClientDbContext> options) : base(options)
    {
    }

    public DbSet<ReservedDomain> ReservedDomains { get; set; } = null!;
    public DbSet<Tunnel> Tunnels { get; set; } = null!;
    public DbSet<InterceptorEntity> Interceptors { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var interceptorValuesComparer = new ValueComparer<Dictionary<string, string>>(
            (c1, c2) => c1!.SequenceEqual(c2!),
            c => c.Aggregate(0,
                (a, v) => HashCode.Combine(a, v.Key.GetHashCode(), v.Value.GetHashCode())),
            c => c.ToDictionary(entry => entry.Key, entry => entry.Value)
            );
        
        modelBuilder.Entity<InterceptorEntity>()
            .Property(x => x.Values)
            .HasConversion(
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<Dictionary<string, string>>(v) ?? new Dictionary<string, string>(),
                interceptorValuesComparer
            );
    }
}