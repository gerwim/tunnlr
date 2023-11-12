using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Tunnlr.Server.Persistence;

public class SqliteServerDbContext : TunnlrServerDbContext
{
    public SqliteServerDbContext(DbContextOptions<SqliteServerDbContext> options) : base(options)
    {
    }
}

public class SqliteDbContextFactory : IDesignTimeDbContextFactory<SqliteServerDbContext>
{
    public SqliteServerDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SqliteServerDbContext>();
        optionsBuilder.UseSqlite("");

        return new SqliteServerDbContext(optionsBuilder.Options);
    }
}