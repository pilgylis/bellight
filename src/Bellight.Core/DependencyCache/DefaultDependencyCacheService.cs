using Bellight.Core.Misc;
using Microsoft.Extensions.Logging;

namespace Bellight.Core.DependencyCache;

public class DefaultDependencyCacheService(
    BellightCoreOptions options,
    ISerializer serializer,
    IServiceProvider serviceProvider) : IDependencyCacheService
{
    public bool Load()
    {
        if (options.DependencyCacheOptions?.Enabled != true)
        {
            return false;
        }

        var filePath = string.IsNullOrEmpty(options.DependencyCacheOptions.FileLocation) ? options.DependencyCacheOptions.FileName :
            Path.Combine(options.DependencyCacheOptions.FileLocation, options.DependencyCacheOptions.FileName);

        var content = LoadCache(filePath);
        if (string.IsNullOrEmpty(content))
        {
            return false;
        }

        var model = serializer.TryDeserializeObject<DependencyCacheModel>(content);

        if (!VerifyAssembles(model.Assemblies!))
        {
            return false;
        }

        if (model.TypeHandlers?.Any() != true)
        {
            return false;
        }

        var loadSuccess = SafeExecute.Sync(() =>
        {
            foreach (var item in model.TypeHandlers)
            {
                var type = Type.GetType(item.Name!);
                var handler = serviceProvider.GetService(type!) as ITypeHandler;
                handler?.LoadCache(item.Sections!);
            }
        });

        return loadSuccess;
    }

    public void Save(DependencyCacheModel item)
    {
        if (options.DependencyCacheOptions?.Enabled != true)
        {
            return;
        }

        if (options.DependencyCacheOptions.PrettyPrint)
        {
            serializer.Settings.WriteIndented = true;
        }

        var serializedContent = serializer.TrySerializeObject(item);
        if (string.IsNullOrEmpty(serializedContent))
        {
            return;
        }

        var filePath = string.IsNullOrEmpty(options.DependencyCacheOptions.FileLocation) ? options.DependencyCacheOptions.FileName :
            Path.Combine(options.DependencyCacheOptions.FileLocation, options.DependencyCacheOptions.FileName);

        try
        {
            File.WriteAllText(filePath, serializedContent);
        }
        catch(Exception ex)
        {
            CoreLogging.Logger?.LogWarning(ex, "Cannot write cache file: {FilePath}. As a consequence, next startup may suffer a performance issue.", filePath);
        }
    }

    private static bool VerifyAssembles(IEnumerable<string> assemblyNames)
    {
        return assemblyNames?.Any() == true;
    }

    private static string LoadCache(string fileName)
    {
        try
        {
            return File.ReadAllText(fileName);
        }
        catch(Exception ex)
        {
            CoreLogging.Logger?.LogWarning(ex, "Cannot open cache file: {FileName}", fileName);
            return string.Empty;
        }
    }
}