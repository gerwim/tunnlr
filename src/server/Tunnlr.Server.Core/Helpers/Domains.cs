using System.Text.RegularExpressions;

namespace Tunnlr.Server.Core.Helpers;

public static partial class Domains
{
    public static string CleanInput(string domain)
    {
        var regex = ValidDomainNameRegex();

        return regex.Replace(domain, string.Empty).ToLowerInvariant();
    }

    /// <summary>
    /// Each level of a domain can be 63 characters at max
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string ValidateAndConvertDomainLength(string input)
    {
        var length = input.Length;
        if (length <= 63) return input;
        
        return input.Substring(length - 63, 63);
    }

    [GeneratedRegex("[^a-zA-Z-0-9]")]
    private static partial Regex ValidDomainNameRegex();
}