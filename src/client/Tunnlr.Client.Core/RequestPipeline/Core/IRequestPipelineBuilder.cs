using Microsoft.Extensions.DependencyInjection;
using Tunnlr.Client.Core.RequestPipeline.Middleware;

namespace Tunnlr.Client.Core.RequestPipeline.Core;


public interface IRequestPipelineBuilder
{
    IServiceCollection Services { get; }
    IServiceProvider? ServiceProvider { get; }
    IRequestPipelineBuilder Add<T>(T middleware) where T : class, IRequestMiddleware;
}