using Microsoft.Extensions.DependencyInjection;
using Tunnlr.Client.Core.RequestPipeline.Middleware;

namespace Tunnlr.Client.Core.RequestPipeline.Core;

public class RequestPipelineBuilder : IRequestPipelineBuilder
{
    public IRequestPipelineBuilder Add<T>(T middleware) where T : class, IRequestMiddleware
    {
        Services.AddScoped<IRequestMiddleware, T>();
        return this;
    }

    public IServiceProvider Build()
    {
        if (ServiceProvider is not null) return ServiceProvider;

        ServiceProvider = Services.BuildServiceProvider();
        return ServiceProvider;
    }
    
    public IServiceCollection Services { get; init; } = new ServiceCollection();
    public IServiceProvider? ServiceProvider { get; private set; }
}