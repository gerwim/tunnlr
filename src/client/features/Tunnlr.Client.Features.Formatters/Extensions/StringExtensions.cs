namespace Tunnlr.Client.Features.Formatters.Extensions;

public static class StringExtensions
{
    public static string ToCleanString(this string input)
    {
        return input.Replace("\r", string.Empty)
            .Replace("\n", string.Empty)
            .Trim();
    }
}