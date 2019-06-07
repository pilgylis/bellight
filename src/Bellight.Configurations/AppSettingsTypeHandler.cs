using Bellight.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

namespace Bellight.Configurations
{
    public class AppSettingsTypeHandler : ITypeHandler
    {
        private static readonly Type AppSettingsInterfaceType = typeof(IAppSettingSection);

        private readonly IServiceCollection _builder;
        private readonly IConfiguration _configuration;
        private readonly MethodInfo _configureMethod;

        public AppSettingsTypeHandler(IServiceCollection builder, IConfiguration configuration)
        {
            _builder = builder;
            _configuration = configuration;

            _configureMethod = typeof(OptionsConfigurationServiceCollectionExtensions)
                .GetMethod("Configure", new[] { typeof(IServiceCollection), typeof(IConfiguration) });
        }

        public void Process(Type type)
        {
            if (!AppSettingsInterfaceType.IsAssignableFrom(type))
            {
                return;
            }

            var sectionName = type.Name;
            var configurationSection = _configuration.GetSection(sectionName);

            if (configurationSection == null)
            {
                return;
            }

            var genericConfigureMethod = _configureMethod.MakeGenericMethod(type);
            genericConfigureMethod.Invoke(null, new object[] { _builder, configurationSection });
        }
    }
}
