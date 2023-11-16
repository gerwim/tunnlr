namespace Tunnlr.Client.Core.RequestPipeline.Core;

public interface IRequestPipelineExecutor
{
    Task ExecuteAsync(HttpRequestMessage requestMessage);
}