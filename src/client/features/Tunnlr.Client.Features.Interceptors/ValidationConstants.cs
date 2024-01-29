namespace Tunnlr.Client.Features.Interceptors;

public class ValidationConstants
{
    public const string HeaderNameRegex = "^[a-zA-Z0-9_-]+$";
    public const string HeaderValueRegex = @"^[^\x00-\x1F:]*$";

    // disallow status 204, we need to clear the Content-Length header and make sure body has been sent yet
    // TODO: implement this
    public const string StatusCodeRegex = "^(?!(?:204))[1-5][0-9][0-9]$";
}