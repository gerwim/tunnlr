using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Tunnlr.Server.Persistence;

public class PostgreSqlDbContext : TunnlrDbContext
{
    public PostgreSqlDbContext(DbContextOptions<PostgreSqlDbContext> options) : base(options)
    {
    }
}

public class PostgreSqlDbContextFactory : IDesignTimeDbContextFactory<PostgreSqlDbContext>
{
    public PostgreSqlDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PostgreSqlDbContext>();
        optionsBuilder.UseNpgsql("");

        return new PostgreSqlDbContext(optionsBuilder.Options);
    }
}