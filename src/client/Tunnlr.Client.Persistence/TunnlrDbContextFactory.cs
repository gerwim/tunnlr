using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Tunnlr.Client.Persistence.Exceptions;

namespace Tunnlr.Client.Persistence;

public class TunnlrDbContextFactory : IDesignTimeDbContextFactory<TunnlrClientDbContext>
{
    public TunnlrClientDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TunnlrClientDbContext>();
        optionsBuilder.UseSqlite(""); // empty connection string, since we don't need an actual connection for creation of migrations

        var dbContext = (TunnlrClientDbContext?)Activator.CreateInstance(typeof(TunnlrClientDbContext), optionsBuilder.Options);
        if (dbContext is null) throw new DbContextCreationException("Could not create DbContext");
        
        return dbContext;
    }
}