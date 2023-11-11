using System.Reflection;

namespace Tunnlr.Common.DependencyInjection;

public static class AssemblyScanner
{
    private static List<Assembly> _loadedAssemblies = new List<Assembly>();

    public static List<Assembly> GetLoadedAssemblies(params string[] scanAssembliesStartsWith)
    {
        if (_loadedAssemblies.Any())
        {
            return _loadedAssemblies;
        }

        LoadAssemblies(scanAssembliesStartsWith);
        return _loadedAssemblies;
    }
        
    private static void LoadAssemblies(params string[] scanAssembliesStartsWith)
    {
        HashSet<Assembly> loadedAssemblies = new HashSet<Assembly>();

        List<string> assembliesToBeLoaded = new List<string>();

        string appDllsDirectory = AppDomain.CurrentDomain.BaseDirectory;

        if (scanAssembliesStartsWith?.Any() == true)
        {
            if (scanAssembliesStartsWith.Length == 1)
            {
                string searchPattern = $"{scanAssembliesStartsWith.First()}*.dll";
                string[] assemblyPaths = Directory.GetFiles(appDllsDirectory, searchPattern, SearchOption.AllDirectories);
                assembliesToBeLoaded.AddRange(assemblyPaths);
            }

            if (scanAssembliesStartsWith.Length > 1)
            {
                foreach (string starsWith in scanAssembliesStartsWith)
                {
                    string searchPattern = $"{starsWith}*.dll";
                    string[] assemblyPaths = Directory.GetFiles(appDllsDirectory, searchPattern, SearchOption.AllDirectories);
                    assembliesToBeLoaded.AddRange(assemblyPaths);
                }
            }
        }
        else
        {
            string[] assemblyPaths = Directory.GetFiles(appDllsDirectory, "*.dll");
            assembliesToBeLoaded.AddRange(assemblyPaths);
        }

        foreach (string path in assembliesToBeLoaded)
        {
            try
            {
#pragma warning disable S3885 // Sonar is incorrect, LoadFrom should be used.
                Assembly assembly = Assembly.LoadFrom(path);
#pragma warning restore S3885
                loadedAssemblies.Add(assembly);
            }
            catch (Exception)
            {
                // Continue loading
            }
        }

        _loadedAssemblies = loadedAssemblies.ToList();
    }
}