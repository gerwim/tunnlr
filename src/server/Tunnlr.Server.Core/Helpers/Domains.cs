using System.Text.RegularExpressions;

namespace Tunnlr.Server.Core.Helpers;

public static partial class Domains
{
    public static string CleanInput(string domain)
    {
        var regex = ValidDomainNameRegex();

        return regex.Replace(domain, string.Empty).ToLowerInvariant();
    }

    [GeneratedRegex("[^a-zA-Z-0-9]")]
    private static partial Regex ValidDomainNameRegex();
}