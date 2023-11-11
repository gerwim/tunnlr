namespace Tunnlr.Common.Options;

public class Auth0Options
{
    public const string OptionKey = "Auth0";
    
    public string? Audience { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? Domain { get; set; }
    public string? Issuer { get; set; }
}