using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Tunnlr.Common.DependencyInjection;

public static class Configurators
{
    public static void RunAll(WebApplication app)
    {
        var configurators = AssemblyScanner.GetLoadedAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => typeof(IConfigurator).IsAssignableFrom(p) && !p.IsAbstract && !p.IsInterface);

        foreach (var type in configurators)
        {
            IConfigurator instance = (IConfigurator)ActivatorUtilities.CreateInstance(app.Services, type);
            instance?.Configure(app);
        }
    }
}