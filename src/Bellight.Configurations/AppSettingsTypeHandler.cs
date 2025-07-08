using Bellight.Core;
using Bellight.Core.DependencyCache;
using Bellight.Core.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Bellight.Configurations;

public class AppSettingsTypeHandler(IServiceCollection builder, IConfiguration configuration) : ITypeHandler
{
    private static readonly Type AppSettingsInterfaceType = typeof(IAppSettingSection);

    private readonly IServiceCollection _builder = builder;
    private readonly IConfiguration _configuration = configuration;
    private readonly MethodInfo _configureMethod = typeof(OptionsConfigurationServiceCollectionExtensions)
            .GetMethod("Configure", [typeof(IServiceCollection), typeof(IConfiguration)])!;

    private readonly IList<KeyValuePair<string, Type>> _allTypes = [];

    public void LoadCache(IEnumerable<TypeHandlerCacheSection> sections)
    {
        var section = sections.FirstOrDefault();
        if (section == null)
        {
            return;
        }

        foreach (var line in section.Lines!)
        {
            var colonIndex = line.IndexOf(':');
            if (colonIndex < 0 || colonIndex >= line.Length)
            {
                throw new BellightStartupException($"Data corrupted: {line}");
            }

            var configurationSectionName = line.Substring(0, colonIndex).Trim();
            var typeName = line.Substring(colonIndex + 1);
            var type = Type.GetType(typeName);

            var configurationSection = _configuration.GetSection(configurationSectionName);

            if (configurationSection == null)
            {
                continue;
            }

            var genericConfigureMethod = _configureMethod.MakeGenericMethod(type!);
            genericConfigureMethod.Invoke(null, [_builder, configurationSection]);
        }
    }

    public void Process(Type type)
    {
        if (!AppSettingsInterfaceType.IsAssignableFrom(type))
        {
            return;
        }

        var configurationSectionName = type.Name;
        var configurationSection = _configuration.GetSection(configurationSectionName);

        if (configurationSection == null)
        {
            return;
        }

        var genericConfigureMethod = _configureMethod.MakeGenericMethod(type);
        genericConfigureMethod.Invoke(null, [_builder, configurationSection]);

        _allTypes.Add(new KeyValuePair<string, Type>(configurationSectionName, type));
    }

    public IEnumerable<TypeHandlerCacheSection> SaveCache()
    {
        yield return new TypeHandlerCacheSection
        {
            Name = "Content",
            Lines = _allTypes.Select(pair => string.Format("{0}: {1}", pair.Key, pair.Value.AssemblyQualifiedName))
        };
    }
}