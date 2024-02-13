using Tunnlr.Client.Core.Contracts.Models;
using Tunnlr.Client.Features.Interceptors.Core;

namespace Tunnlr.Client.Features.Interceptors.RequestInterceptors;

public class RemoveRequestHeaderInterceptor : IRequestInterceptor
{
    public string FriendlyName => "Remove request header";
    public string Subtitle => $"Header name: {HeaderName}";

    [InterceptorProperty(DisplayName = "Header name", PropertyType = PropertyType.Text, ValidationRegex = ValidationConstants.HeaderNameRegex)]
    public string HeaderName { get; set; } = string.Empty;
    
    public Task InvokeAsync(IRequest request)
    {
        request.HttpRequest.Headers.Remove(HeaderName);

        return Task.CompletedTask;
    }
}