namespace Tunnlr.Client.Web.Helpers;

public static class RequestDurationFormatter
{
    public static string FormatRequestDuration(DateTime start, DateTime end)
    {
        var time = TimeSpan.FromMilliseconds((end-start).TotalMilliseconds);

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
            formattedTime += $"{time.Seconds}s";
        }
        if (time.Milliseconds > 0)
        {
            formattedTime += $"{time.Milliseconds}ms";
        }

        return formattedTime;
    }
}