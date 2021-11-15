using Bellight.Core;
using Bellight.Core.DependencyCache;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Bellight.Configurations
{
    public class AppSettingsTypeHandler : ITypeHandler
    {
        private static readonly Type AppSettingsInterfaceType = typeof(IAppSettingSection);

        private readonly IServiceCollection _builder;
        private readonly IConfiguration _configuration;
        private readonly MethodInfo _configureMethod;

        private readonly IList<KeyValuePair<string, Type>> _allTypes = new List<KeyValuePair<string, Type>>();

        public AppSettingsTypeHandler(IServiceCollection builder, IConfiguration configuration)
        {
            _builder = builder;
            _configuration = configuration;

            _configureMethod = typeof(OptionsConfigurationServiceCollectionExtensions)
                .GetMethod("Configure", new[] { typeof(IServiceCollection), typeof(IConfiguration) })!;
        }

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
                    throw new Exception($"Data corrupted: {line}");
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
                genericConfigureMethod.Invoke(null, new object[] { _builder, configurationSection });
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
            genericConfigureMethod.Invoke(null, new object[] { _builder, configurationSection });

            _allTypes.Add(new KeyValuePair<string, Type>(configurationSectionName, type));
        }

        public IEnumerable<TypeHandlerCacheSection> SaveCache()
        {
            return new List<TypeHandlerCacheSection> {
                new TypeHandlerCacheSection
                {
                    Name = "Content",
                    Lines = _allTypes.Select(pair => string.Format("{0}: {1}", pair.Key, pair.Value.AssemblyQualifiedName))
                }
            };
        }
    }
}
