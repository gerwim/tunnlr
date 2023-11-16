using Tunnlr.Client.Core.RequestPipeline.Core;
using Tunnlr.Common.DependencyInjection;

namespace Tunnlr.Client.Core.DependencyInjection;

public static class AutoRegistration
{
    public static IRequestPipelineBuilder RunAllRequestBuilders(this IRequestPipelineBuilder builder)
    {
        var types = AssemblyScanner.GetLoadedAssemblies("Tunnlr.Client")
            .SelectMany(s => s.GetTypes())
            .Where(p => typeof(IRequestBuilder).IsAssignableFrom(p) && !p.IsAbstract && !p.IsInterface);

        foreach (var type in types)
        {
            IRequestBuilder? instance = (IRequestBuilder?)Activator.CreateInstance(type);
            instance?.Add(builder);
        }

        return builder;
    }
}