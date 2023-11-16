namespace Tunnlr.Client.Core.RequestPipeline.Middleware;

public interface IRequestMiddleware
{
    Task InvokeAsync(HttpRequestMessage requestMessage);
}