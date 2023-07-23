using System.Net.Http.Headers;
using Google.Protobuf.Collections;
using HttpMethod = Tunnlr.Common.Protobuf.HttpMethod;

namespace Tunnlr.Common;

public static class Helpers
{
    /// <summary>
    /// Converts a string to HttpMethod
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static HttpMethod ConvertToHttpMethod(this string value)
    {
        try
        {
            return Enum.Parse<HttpMethod>(value, true);
        }
        catch
        {
            return HttpMethod.Unknown;
        }
    }

    /// <summary>
    /// Maps headers from .NET to internal models
    /// </summary>
    /// <param name="resultHeaders"></param>
    /// <param name="httpContentHeaders"></param>
    /// <param name="responseHeaders"></param>
    public static void MapHeaders(HttpResponseHeaders resultHeaders, HttpContentHeaders httpContentHeaders,
        MapField<string, string> responseHeaders)
    {
        foreach (var httpRequestHeader in resultHeaders)
        {
            responseHeaders.Add(httpRequestHeader.Key, httpRequestHeader.Value.First());
        }
        
        foreach (var httpRequestHeader in httpContentHeaders)
        {
            responseHeaders.Add(httpRequestHeader.Key, httpRequestHeader.Value.First());
        }
    }
}