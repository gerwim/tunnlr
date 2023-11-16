using System.Globalization;

namespace Tunnlr.Client.Web.Extensions;

public static class DoubleExtentions
{
    public static string ToInvariantString(this double input) => input.ToString(CultureInfo.InvariantCulture);
}