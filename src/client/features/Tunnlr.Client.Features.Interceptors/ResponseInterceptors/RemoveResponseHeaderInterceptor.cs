using Tunnlr.Client.Core.Models;
using Tunnlr.Client.Features.Interceptors.Core;

namespace Tunnlr.Client.Features.Interceptors.ResponseInterceptors;

public class RemoveResponseHeaderInterceptor : IResponseInterceptor
{
    public string FriendlyName => "Remove response header";
    public string Subtitle => $"Header name: {HeaderName}";
    
    [InterceptorProperty(DisplayName = "Header name", PropertyType = PropertyType.Text, ValidationRegex = ValidationConstants.HeaderNameRegex)]
    public string HeaderName { get; set; } = string.Empty;
    
    
    public Task InvokeAsync(IResponse response)
    {
        response.HttpResponse.Headers.Remove(HeaderName);
        
        return Task.CompletedTask;
    }
}