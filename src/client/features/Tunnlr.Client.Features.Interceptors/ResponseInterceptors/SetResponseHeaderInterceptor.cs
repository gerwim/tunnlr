using Tunnlr.Client.Core.Models;
using Tunnlr.Client.Features.Interceptors.Core;

namespace Tunnlr.Client.Features.Interceptors.ResponseInterceptors;

public class SetResponseHeaderInterceptor : IResponseInterceptor
{
    public string FriendlyName => "Set response header";
    public string Subtitle => $"Header name: {HeaderName}";

    [UiElement(DisplayName = "Header name", ElementType = UiElementTypes.TextInput, ValidationRegex = ValidationConstants.HeaderNameRegex)]
    public string HeaderName { get; set; } = string.Empty;
    
    [UiElement(DisplayName = "Header value", ElementType = UiElementTypes.TextInput, ValidationRegex = ValidationConstants.HeaderValueRegex)]
    public string HeaderValue { get; set; } = string.Empty;
    
    public Task InvokeAsync(IResponse response)
    {
        response.HttpResponse.Headers[HeaderName] = HeaderValue;

        return Task.CompletedTask;
    }
}