using System.Security.Claims;

namespace Tunnlr.Server.Core.Authentication;

public interface IAuthenticationService
{
    Task<ClaimsPrincipal?> ValidateJwt(string jwt);
}