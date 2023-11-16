using Tunnlr.Client.Core.RequestPipeline.Core;

namespace Tunnlr.Client.Core.DependencyInjection;

public interface IRequestBuilder
{
    public void Add(IRequestPipelineBuilder builder);
}