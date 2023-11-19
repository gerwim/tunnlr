using Microsoft.Extensions.DependencyInjection;
using Tunnlr.Client.Features.Interceptors.Core;
using Tunnlr.Client.Features.Interceptors.Exceptions;

namespace Tunnlr.Client.Features.Interceptors;

public static class InterceptorFactory
{
    public static IInterceptor FromType(IServiceProvider serviceProvider, Type type, Dictionary<string, string> values)
    {
        if (!type.GetInterfaces().Contains(typeof(IInterceptor))) throw new InvalidInterceptorTypeException("This is not a valid interceptor type");

        var instance = (IInterceptor) ActivatorUtilities.CreateInstance(serviceProvider, type);
        var properties = type.GetProperties();
        foreach (var property in properties)
        {
            if (values.TryGetValue(property.Name, out var value))
            {
                property.SetValue(instance, value);
            }
        }

        return instance;
    }
}