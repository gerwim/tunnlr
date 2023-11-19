using Microsoft.AspNetCore.Authentication;

namespace Tunnlr.Client.Web.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuthenticationService> _logger;
    
    public AuthenticationService(IHttpContextAccessor httpContextAccessor, ILogger<AuthenticationService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        
        if (_httpContextAccessor.HttpContext is null) _logger.LogError("HttpContext is null");
    }

    public bool IsAuthenticated()
    {
        return _httpContextAccessor.HttpContext!.User.Identity?.IsAuthenticated ?? false;
    }

    public string? GetName()
    {
        return _httpContextAccessor.HttpContext!.User.Identity?.Name;
    }

    public Task<string?> GetAccessToken()
    {
        try
        {
            return _httpContextAccessor.HttpContext!.GetTokenAsync("access_token");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not retrieve access token");
            return Task.FromResult<string?>(null);
        }
    }
}