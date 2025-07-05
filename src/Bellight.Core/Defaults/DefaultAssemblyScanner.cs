using Bellight.Core.DependencyCache;
using System.Reflection;

namespace Bellight.Core.Defaults;

public class DefaultAssemblyScanner(
    BellightCoreOptions options,
    IAssemblyLoader assemblyLoader,
    IAssemblyHandler assemblyHandler,
    IEnumerable<ITypeHandler> typeHandlers) : IAssemblyScanner
{

    public DependencyCacheModel Scan()
    {
        var thisAssembly = GetType().GetTypeInfo().Assembly; // the current Bellight.Core assembly

        var assemblies = new List<Assembly> { thisAssembly };

        var thisAssemblyName = thisAssembly.GetName().Name;

        var loadedAssemblies = assemblyLoader.Load()
            .Where(a => assemblyPredicate(a, thisAssemblyName!));

        var enumerable = loadedAssemblies as Assembly[] ?? loadedAssemblies.ToArray();
        if (enumerable.Length > 0)
        {
            assemblies.AddRange(enumerable);
        }

        if (options.AdditionalAssemblies?.Any() == true)
        {
            assemblies.AddRange(options.AdditionalAssemblies);
        }

        foreach (var assembly in assemblies)
        {
            assemblyHandler.Process(assembly);
        }

        return new DependencyCacheModel
        {
            Assemblies = assemblies.Select(a => a.FullName)!,
            TypeHandlers = typeHandlers.Select(h => new TypeHandlerCacheModel
            {
                Name = h.GetType().AssemblyQualifiedName,
                Sections = h.SaveCache()
            })
        };
    }

    private readonly Func<Assembly, string, bool> assemblyPredicate = (a, b)
        => a.GetReferencedAssemblies().Any(asb => string.CompareOrdinal(asb.Name, b) == 0);
}