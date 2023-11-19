using Tunnlr.Client.Core.Models;

namespace Tunnlr.Client.Features.Interceptors.Core;

public interface IResponseInterceptor : IInterceptor
{
    Task InvokeAsync(IResponse response);
}