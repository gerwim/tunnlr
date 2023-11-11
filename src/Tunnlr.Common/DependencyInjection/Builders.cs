using Microsoft.AspNetCore.Builder;

namespace Tunnlr.Common.DependencyInjection;

public static class Builders
{
    public static void RunAll(WebApplicationBuilder builder)
    {
        var builders = AssemblyScanner.GetLoadedAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => typeof(IBuilder).IsAssignableFrom(p) && !p.IsAbstract && !p.IsInterface);

        foreach (var type in builders)
        {
            IBuilder? instance = (IBuilder?)Activator.CreateInstance(type);
            instance?.Build(builder);
        }
    }
}