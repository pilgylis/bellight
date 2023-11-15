using Bellight.Core.DependencyCache;
using System.Reflection;

namespace Bellight.Core.Defaults;

public class DefaultAssemblyScanner(
    BellightCoreOptions options,
    IAssemblyLoader assemblyLoader,
    IAssemblyHandler assemblyHandler,
    IEnumerable<ITypeHandler> typeHandlers) : IAssemblyScanner
{
    private readonly IAssemblyLoader _assemblyLoader = assemblyLoader;
    private readonly IAssemblyHandler _assemblyHandler = assemblyHandler;

    public DependencyCacheModel Scan()
    {
        var thisAssembly = GetType().GetTypeInfo().Assembly; // the current Bellight.Core assembly

        var assemblies = new List<Assembly> { thisAssembly };

        var thisAssemblyName = thisAssembly.GetName().Name;

        var loadedAssemblies = _assemblyLoader.Load()
            .Where(a => assemblyPredicate(a, thisAssemblyName!));

        if (loadedAssemblies?.Any() == true)
        {
            assemblies.AddRange(loadedAssemblies);
        }

        if (_options.AdditionalAssemblies?.Any() == true)
        {
            assemblies.AddRange(_options.AdditionalAssemblies);
        }

        foreach (var assembly in assemblies)
        {
            _assemblyHandler.Process(assembly);
        }

        return new DependencyCacheModel
        {
            Assemblies = assemblies.Select(a => a.FullName)!,
            TypeHandlers = _typeHandlers.Select(h => new TypeHandlerCacheModel
            {
                Name = h.GetType().AssemblyQualifiedName,
                Sections = h.SaveCache()
            })
        };
    }

    private readonly Func<Assembly, string, bool> assemblyPredicate = (a, b)
        => a.GetReferencedAssemblies().Any(asb => string.CompareOrdinal(asb.Name, b) == 0);

    private readonly BellightCoreOptions _options = options;
    private readonly IEnumerable<ITypeHandler> _typeHandlers = typeHandlers;
}