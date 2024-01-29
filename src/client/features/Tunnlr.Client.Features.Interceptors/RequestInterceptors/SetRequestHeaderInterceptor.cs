using Tunnlr.Client.Core.Contracts.Models;
using Tunnlr.Client.Features.Interceptors.Core;

namespace Tunnlr.Client.Features.Interceptors.RequestInterceptors;

public class SetRequestHeaderInterceptor : IRequestInterceptor
{
    public string FriendlyName => "Set request header";
    public string Subtitle => $"Header name: {HeaderName}";

    [UiElement(DisplayName = "Header name", ElementType = UiElementTypes.TextInput, ValidationRegex = ValidationConstants.HeaderNameRegex)]
    public string HeaderName { get; set; } = string.Empty;
    
    [UiElement(DisplayName = "Header value", ElementType = UiElementTypes.TextInput, ValidationRegex = ValidationConstants.HeaderValueRegex)]
    public string HeaderValue { get; set; } = string.Empty;
    
    public Task InvokeAsync(IRequest request)
    {
        request.HttpRequest.Headers[HeaderName] = HeaderValue;

        return Task.CompletedTask;
    }
}