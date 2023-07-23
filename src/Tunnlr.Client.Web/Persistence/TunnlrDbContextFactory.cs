using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Tunnlr.Client.Web.Persistence.Exceptions;

namespace Tunnlr.Client.Web.Persistence;

public class TunnlrDbContextFactory : IDesignTimeDbContextFactory<TunnlrDbContext>
{
    public TunnlrDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TunnlrDbContext>();
        optionsBuilder.UseSqlite(""); // empty connection string, since we don't need an actual connection for creation of migrations

        var dbContext = (TunnlrDbContext?)Activator.CreateInstance(typeof(TunnlrDbContext), optionsBuilder.Options);
        if (dbContext is null) throw new DbContextCreationException("Could not create DbContext");
        
        return dbContext;
    }
}