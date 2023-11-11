using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GerwimFeiken.Cache;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Tunnlr.Common.Exceptions;
using Tunnlr.Common.Options;

namespace Tunnlr.Server.Core.Authentication;

public class AuthenticationService : IAuthenticationService
{
    private readonly ICache _cache;
    private readonly string _auth0Audience;
    private readonly string _auth0Domain;
    private readonly string _auth0Issuer;
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(HttpClient httpClient, ICache cache, IOptions<Auth0Options> options, ILogger<AuthenticationService> logger)
    {
        _cache = cache;
        _logger = logger;
        _auth0Audience = options.Value.Audience ?? throw new InvalidConfigurationException($"{nameof(options.Value.Audience)} is empty");
        _auth0Domain = options.Value.Domain ?? throw new InvalidConfigurationException($"{nameof(options.Value.Domain)} is empty");
        _auth0Issuer = options.Value.Issuer ?? throw new InvalidConfigurationException($"{nameof(options.Value.Issuer)} is empty");
        _httpClient = httpClient;
    }

    public async Task<ClaimsPrincipal?> ValidateJwt(string jwt)
    {
        var jwks = await _cache.ReadOrWrite(
            "Auth0Jwks",
            async () => await _httpClient.GetStringAsync($"https://{_auth0Domain}/.well-known/jwks.json"),
            TimeSpan.FromDays(1));
        
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidIssuer = _auth0Issuer,
            ValidAudience = _auth0Audience,
            IssuerSigningKeys = new JsonWebKeySet(jwks).GetSigningKeys(),
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
        };

        try
        {
            return tokenHandler.ValidateToken(jwt, validationParameters, out _);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not validate jwt");
        }

        return null;
    }
}
