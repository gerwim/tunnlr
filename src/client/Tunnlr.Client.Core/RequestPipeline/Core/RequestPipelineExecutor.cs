using Microsoft.Extensions.DependencyInjection;
using Tunnlr.Client.Core.Exceptions;
using Tunnlr.Client.Core.RequestPipeline.Middleware;

namespace Tunnlr.Client.Core.RequestPipeline.Core;

public class RequestPipelineExecutor : IRequestPipelineExecutor
{
    private readonly IRequestPipelineBuilder _requestPipelineBuilder;

    public RequestPipelineExecutor(IRequestPipelineBuilder requestPipelineBuilder)
    {
        _requestPipelineBuilder = requestPipelineBuilder;
    }

    public async Task ExecuteAsync(HttpRequestMessage requestMessage)
    {
        using var scope = _requestPipelineBuilder.ServiceProvider?.CreateScope();
        if (scope is null) throw new RequestPipelineException("Could not create executor scope");

        var services = scope.ServiceProvider.GetServices<IRequestMiddleware>();
        foreach (var requestMiddleware in services)
        {
            await requestMiddleware.InvokeAsync(requestMessage).ConfigureAwait(false);
        }
    }
}