using Microsoft.Extensions.DependencyInjection;
using Tunnlr.Client.Core.DependencyInjection;
using Tunnlr.Client.Core.RequestPipeline.Core;

namespace Tunnlr.Client.Core.RequestPipeline;

public static class RequestPipelineExtension
{
    public static IRequestPipelineBuilder AddRequestPipelineExecutor(this IServiceCollection serviceCollection)
    {
        var pipelineBuilder = new RequestPipelineBuilder();
        serviceCollection.AddSingleton<IRequestPipelineBuilder>(pipelineBuilder);
        pipelineBuilder.RunAllRequestBuilders();
        pipelineBuilder.Build();

        serviceCollection.AddScoped<IRequestPipelineExecutor, RequestPipelineExecutor>();
        return pipelineBuilder;
    }
}