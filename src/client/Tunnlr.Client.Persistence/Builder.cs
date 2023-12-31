using Microsoft.AspNetCore.Builder;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tunnlr.Client.Utilities;
using Tunnlr.Common.DependencyInjection;

namespace Tunnlr.Client.Persistence;

public class Builder : IBuilder
{
    public void Build(WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<TunnlrClientDbContext>(options =>
        {
            var directory = Storage.GetTunnlrStorageDirectory();
            var connectionStringBuilder = new SqliteConnectionStringBuilder($"Data Source={Path.Combine(directory, "Tunnlr.Client.Web.db")}"); 
            options.UseSqlite(connectionStringBuilder.ToString(), sqliteOptions =>
            {
                sqliteOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);
            });

#if DEBUG
            options.EnableDetailedErrors();
            options.EnableSensitiveDataLogging();
#endif
            // Set default tracking
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });
    }
}