using Microsoft.Extensions.Logging;
using Tunnlr.Client.Core.Models;
using Tunnlr.Client.Features.Interceptors.Core;

namespace Tunnlr.Client.Features.Interceptors.ResponseInterceptors;

public class StatusCodeResponseInterceptor : IResponseInterceptor
{
    private readonly ILogger<StatusCodeResponseInterceptor> _logger;

    public StatusCodeResponseInterceptor(ILogger<StatusCodeResponseInterceptor> logger)
    {
        _logger = logger;
    }
    
    public string FriendlyName => "Set response status code";
    public string Subtitle => $"Statuscode: {Statuscode}";

    [InterceptorProperty(DisplayName = "Statuscode", PropertyType = PropertyType.Numbers, ValidationRegex = ValidationConstants.StatusCodeRegex)]
    public string Statuscode { get; set; } = string.Empty;

    public Task InvokeAsync(IResponse response)
    {
        try
        {
            var value = Int32.Parse(Statuscode);
            response.HttpResponse.StatusCode = value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Could not convert status code {Statuscode} to an integer");
        }

        return Task.CompletedTask;
    }
}