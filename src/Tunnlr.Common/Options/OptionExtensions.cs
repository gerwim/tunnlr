using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tunnlr.Common.Exceptions;

namespace Tunnlr.Common.Options;

public static class OptionExtensions
{
    [return: NotNull]
    public static T GetRequiredOptions<T>(this IConfiguration configuration, string key) where T : IOptions, new()
    {
        var configurationSection = configuration.GetRequiredSection(key);
        var data = new T();
        configurationSection.Bind(data);
        return data;
    }
    
    [return: NotNull]
    public static T GetRequiredValue<T>(this IConfigurationSection options, string key)
    {
        var value = options.GetValue<T>(key);

        if ((typeof(T) == typeof(string) && string.IsNullOrWhiteSpace(value as string))
            || value is null)
        {
            throw new InvalidConfigurationException($"Setting {key} is empty. Please set the value in {options.Path}");
        }
        return value;
    }
    
    [return: NotNull]
    public static TValue GetRequiredValue<TClass, TValue>(this TClass options, Expression<Func<TClass, TValue>> expression) 
        where TClass : IOptions
    {
        var propertyName = GetNameFromMemberExpression(expression.Body);
        var type = typeof(TClass);
        var properties = type.IsInterface
            ? new[] { type }
                .Concat(type.GetInterfaces())
                .SelectMany(i => i.GetProperties())
            : type.GetProperties();
        
        var value = properties.FirstOrDefault(x => x.Name == propertyName)?.GetValue(options, null);

        return value switch
        {
            null => throw new InvalidConfigurationException($"Setting {propertyName} is empty."),
            "" => throw new InvalidConfigurationException($"Setting {propertyName} is empty."),
            TValue cast => cast,
            _ => throw new InvalidConfigurationException($"Setting {propertyName} is not of type {typeof(TClass)}.")
        };
    }

    /// <summary>
    /// Registers the section and returns a bound version of it
    /// </summary>
    /// <param name="section"></param>
    /// <param name="builder"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T RegisterOptions<T>(this IConfigurationSection section, WebApplicationBuilder builder) where T : class, IOptions, new()
    {
        var options = new T();
        builder.Services.Configure<T>(section);
        section.Bind(options);
        return options;
    }
    
    private static string GetNameFromMemberExpression(Expression expression)
    {
        return expression switch
        {
            MemberExpression memberExpression => memberExpression.Member.Name,
            UnaryExpression unaryExpression => GetNameFromMemberExpression(unaryExpression.Operand),
            // ConditionalExpression conditionalExpression => Expression.Lambda<Func<string>>(conditionalExpression).Compile()(), // TODO: implement this (see other todo in Infrastructure/Olympia.ApiGateway.Infrastructure.Common/Builder.cs)
            _ => throw new InvalidConfigurationException($"Invalid property type, this exception should never happen.")
        };
    }
}