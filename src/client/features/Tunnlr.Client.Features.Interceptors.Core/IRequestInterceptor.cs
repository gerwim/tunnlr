using Tunnlr.Client.Core.Contracts.Models;

namespace Tunnlr.Client.Features.Interceptors.Core;

public interface IRequestInterceptor : IInterceptor
{
    Task InvokeAsync(IRequest request);
}