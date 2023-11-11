using Microsoft.AspNetCore.Builder;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Tunnlr.Common.DependencyInjection;
using Tunnlr.Common.Exceptions;
using Tunnlr.Common.Options;
using Tunnlr.Server.Persistence.Options;

namespace Tunnlr.Server.Persistence;

public class Builder : IBuilder
{
    public void Build(WebApplicationBuilder builder)
    {
        var options = builder.Configuration.GetRequiredSection(PersistenceOptions.OptionKey).RegisterOptions<PersistenceOptions>(builder);

        var provider = options.GetRequiredValue(x => x.Provider);
        switch (provider.ToLowerInvariant())
        {
            case "sqlite":
                AddSqliteDbContext(builder, options);
                break;
            case "postgresql":
                AddPostgreSqlDbContext(builder, options);
                break;
            default:
                throw new InvalidConfigurationException($"{provider} is not a valid database provider");
        }

    }

    private static void AddSqliteDbContext(WebApplicationBuilder builder, PersistenceOptions persistenceOptions)
    {
        builder.Services.AddDbContext<TunnlrDbContext, SqliteDbContext>(options =>
        {
            // Create a custom connection string for each Sqlite database since Sqlite does not support schemas
            var connectionStringBuilder = new SqliteConnectionStringBuilder(persistenceOptions.ConnectionString);
            
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
    
    private static void AddPostgreSqlDbContext(WebApplicationBuilder builder, PersistenceOptions persistenceOptions)
    {
        builder.Services.AddDbContext<TunnlrDbContext, PostgreSqlDbContext>(options =>
        {
            // Create a custom connection string for each Sqlite database since Sqlite does not support schemas
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(persistenceOptions.ConnectionString);
            
            options.UseNpgsql(connectionStringBuilder.ToString(), npgsqlOptions =>
            {
                npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);
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