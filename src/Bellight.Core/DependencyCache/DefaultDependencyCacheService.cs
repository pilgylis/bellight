using Bellight.Core.Misc;
using Microsoft.Extensions.Logging;

namespace Bellight.Core.DependencyCache;

public class DefaultDependencyCacheService : IDependencyCacheService
{
    private readonly BellightCoreOptions _options;
    private readonly ISerializer _serializer;
    private readonly IServiceProvider _serviceProvider;

    public DefaultDependencyCacheService(
        BellightCoreOptions options,
        ISerializer serializer,
        IServiceProvider serviceProvider)
    {
        _options = options;
        _serializer = serializer;
        _serviceProvider = serviceProvider;
    }

    public bool Load()
    {
        if (_options.DependencyCacheOptions?.Enabled != true)
        {
            return false;
        }

        var filePath = string.IsNullOrEmpty(_options.DependencyCacheOptions.FileLocation) ? _options.DependencyCacheOptions.FileName :
            Path.Combine(_options.DependencyCacheOptions.FileLocation, _options.DependencyCacheOptions.FileName);

        var content = LoadCache(filePath);
        if (string.IsNullOrEmpty(content))
        {
            return false;
        }

        var model = _serializer.TryDeserializeObject<DependencyCacheModel>(content);

        if (model == null || !VerifyAssembles(model.Assemblies!))
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
                var handler = _serviceProvider.GetService(type!) as ITypeHandler;
                handler?.LoadCache(item.Sections!);
            }
        });

        return loadSuccess;
    }

    public void Save(DependencyCacheModel item)
    {
        if (item == null || _options.DependencyCacheOptions?.Enabled != true)
        {
            return;
        }

        if (_options.DependencyCacheOptions.PrettyPrint)
        {
            _serializer.Settings.WriteIndented = true;
        }

        var serializedContent = _serializer.TrySerializeObject(item);
        if (string.IsNullOrEmpty(serializedContent))
        {
            return;
        }

        var filePath = string.IsNullOrEmpty(_options.DependencyCacheOptions.FileLocation) ? _options.DependencyCacheOptions.FileName :
            Path.Combine(_options.DependencyCacheOptions.FileLocation, _options.DependencyCacheOptions.FileName);

        try
        {
            File.WriteAllText(filePath, serializedContent);
        }
        catch
        {
            CoreLogging.Logger?.LogWarning("Cannot write cache file: {filePath}. As a consequence, next startup may suffer a performance issue.", filePath);
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
        catch
        {
            CoreLogging.Logger?.LogWarning("Cannot open cache file: {fileName}", fileName);
            return string.Empty;
        }
    }
}