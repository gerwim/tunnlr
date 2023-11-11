using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Tunnlr.Server.Persistence;

public class SqliteDbContext : TunnlrDbContext
{
    public SqliteDbContext(DbContextOptions<SqliteDbContext> options) : base(options)
    {
    }
}

public class SqliteDbContextFactory : IDesignTimeDbContextFactory<SqliteDbContext>
{
    public SqliteDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SqliteDbContext>();
        optionsBuilder.UseSqlite("");

        return new SqliteDbContext(optionsBuilder.Options);
    }
}