using Tunnlr.Client.Core.Contracts.Models;
using Tunnlr.Client.Features.Interceptors.Core;

namespace Tunnlr.Client.Features.Interceptors.RequestInterceptors;

public class SetRequestHeaderInterceptor : IRequestInterceptor
{
    public string FriendlyName => "Set request header";
    public string Subtitle => $"Header name: {HeaderName}";

    [InterceptorProperty(DisplayName = "Header name", PropertyType = PropertyType.Text, ValidationRegex = ValidationConstants.HeaderNameRegex)]
    public string HeaderName { get; set; } = string.Empty;
    
    [InterceptorProperty(DisplayName = "Header value", PropertyType = PropertyType.Text, ValidationRegex = ValidationConstants.HeaderValueRegex)]
    public string HeaderValue { get; set; } = string.Empty;
    
    public Task InvokeAsync(IRequest request)
    {
        request.HttpRequest.Headers[HeaderName] = HeaderValue;

        return Task.CompletedTask;
    }
}