using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Tunnlr.Common.Options;

public static class OptionExtensions
{
    /// <summary>
    /// Registers the section and returns a bound version of it
    /// </summary>
    /// <param name="section"></param>
    /// <param name="builder"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T RegisterOptions<T>(this IConfigurationSection section, WebApplicationBuilder builder) where T : class, new()
    {
        var options = new T();
        builder.Services.Configure<T>(section);
        section.Bind(options);
        return options;
    }
}