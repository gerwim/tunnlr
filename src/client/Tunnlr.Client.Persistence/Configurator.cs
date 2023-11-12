using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tunnlr.Common.DependencyInjection;

namespace Tunnlr.Client.Persistence;

public class Configurator : IConfigurator
{
    public void Configure(WebApplication app)
    {
        // Apply database migrations
        using var scope = app.Services.CreateScope();
        var dataContext = scope.ServiceProvider.GetRequiredService<TunnlrClientDbContext>();
        dataContext.Database.Migrate();
        dataContext.SaveChanges();
    }
}