using Bellight.Core.Misc;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Bellight.Core.Defaults;

public class DefaultAssemblyLoader : IAssemblyLoader
{
    private static readonly string ThisAssemblyName = 
        typeof(DefaultAssemblyLoader).GetTypeInfo().Assembly.GetQualifiedName();

    public Assembly[] Load()
    {
        var dependencies = DependencyContext.Default?.RuntimeLibraries!;

        var assemblies = new List<Assembly>();
        var entryAssembly = Assembly.GetEntryAssembly();
        var entryAssemblyName = entryAssembly?.GetShortName(); // in case of unit testing, the entry assembly is 'testhost'

        if ("testhost".Equals(entryAssemblyName, StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                entryAssembly = Assembly.Load(new AssemblyName(dependencies[0].Name));
                entryAssemblyName = entryAssembly.GetShortName();
            }
            catch (Exception ex)
            {
                CoreLogging.Logger?.LogWarning(ex, "Error occurred: {errorMessage}", ex.Message);
            }
        }

        var directDependencies = (
            from library in dependencies
            where library.Name.Equals(entryAssemblyName, StringComparison.OrdinalIgnoreCase)
                || library.Name.Equals(ThisAssemblyName, StringComparison.OrdinalIgnoreCase)
                || library.Dependencies.Any(d =>
                    d.Name.Equals(ThisAssemblyName, StringComparison.OrdinalIgnoreCase))
            select library).ToList();

        var entryLibrary = directDependencies.Find(d => d.Name.Equals(entryAssemblyName, StringComparison.OrdinalIgnoreCase));

        //
        if (entryLibrary != null)
        {
            foreach (var entryDependency in entryLibrary
                .Dependencies
                .Where(dependency => !directDependencies.Any(d => d.Name.Equals(dependency.Name, StringComparison.OrdinalIgnoreCase))))
            {
                try
                {
                    var assembly = Assembly.Load(new AssemblyName(entryDependency.Name));
                    assemblies.Add(assembly);
                }
                catch
                {
                    // ignore
                }
            }
        }

        foreach (var directDependency in directDependencies)
        {
            try
            {
                var assembly = Assembly.Load(new AssemblyName(directDependency.Name));
                assemblies.Add(assembly);
            }
            catch
            {
                // ignore
            }
        }

        return [.. assemblies];
    }
}