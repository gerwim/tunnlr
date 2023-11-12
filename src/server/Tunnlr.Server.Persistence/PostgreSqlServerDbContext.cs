using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Tunnlr.Server.Persistence;

public class PostgreSqlServerDbContext : TunnlrServerDbContext
{
    public PostgreSqlServerDbContext(DbContextOptions<PostgreSqlServerDbContext> options) : base(options)
    {
    }
}

public class PostgreSqlDbContextFactory : IDesignTimeDbContextFactory<PostgreSqlServerDbContext>
{
    public PostgreSqlServerDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PostgreSqlServerDbContext>();
        optionsBuilder.UseNpgsql("");

        return new PostgreSqlServerDbContext(optionsBuilder.Options);
    }
}