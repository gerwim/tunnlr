using Tunnlr.Client.Core.Models;
using Tunnlr.Client.Features.Interceptors.Core;

namespace Tunnlr.Client.Features.Interceptors.ResponseInterceptors;

public class SetResponseHeaderInterceptor : IResponseInterceptor
{
    public string FriendlyName => "Set response header";
    public string Subtitle => $"Header name: {HeaderName}";

    [InterceptorProperty(DisplayName = "Header name", PropertyType = PropertyType.Text, ValidationRegex = ValidationConstants.HeaderNameRegex)]
    public string HeaderName { get; set; } = string.Empty;
    
    [InterceptorProperty(DisplayName = "Header value", PropertyType = PropertyType.Text, ValidationRegex = ValidationConstants.HeaderValueRegex)]
    public string HeaderValue { get; set; } = string.Empty;
    
    public Task InvokeAsync(IResponse response)
    {
        response.HttpResponse.Headers[HeaderName] = HeaderValue;

        return Task.CompletedTask;
    }
}