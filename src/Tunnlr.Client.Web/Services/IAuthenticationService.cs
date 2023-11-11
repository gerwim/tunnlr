namespace Tunnlr.Client.Web.Services;

public interface IAuthenticationService
{
    bool IsAuthenticated();
    string? GetName();
    Task<string?> GetAccessToken();
}