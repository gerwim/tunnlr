using Tunnlr.Client.Web.Extensions;

namespace Tunnlr.Client.Web.Helpers;

public static class RequestDurationFormatter
{
    public static string FormatRequestDuration(DateTime start, DateTime end)
    {
        var totalMilliseconds = (end - start).TotalMilliseconds;
        var time = TimeSpan.FromMilliseconds(totalMilliseconds);

        var formattedTime = "";
        if (time.Days > 0)
        {
            formattedTime += $"{time.Days}d";
        }
        if (time.Hours > 0)
        {
            formattedTime += $"{time.Hours}h";
        }
        if (time.Minutes > 0)
        {
            formattedTime += $"{time.Minutes}m";
        }
        if (time.Seconds > 0)
        {
            if (totalMilliseconds is > 1000 and < 60000)
            {
                var ms = TimeSpan.FromSeconds(time.Seconds).Add(TimeSpan.FromMilliseconds(time.Milliseconds));
                formattedTime += $"{Math.Round(ms.TotalSeconds, 1, MidpointRounding.AwayFromZero).ToInvariantString()}s";
            }
            else
            {
                formattedTime += $"{time.Seconds}s";
            }
        }
        if (time.Milliseconds > 0 && totalMilliseconds < 1000)
        {
            formattedTime += $"{time.Milliseconds}ms";
        }

        return formattedTime;
    }
}