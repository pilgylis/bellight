using Bellight.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bellight.Core
{
    public static class BellightCoreOptionExtensions
    {
        public static BellightCoreOptions AddConfigurations(this BellightCoreOptions options, IConfiguration configuration)
        {
            return options
                .AddStartupServiceAction(startupContainerServices => {
                    startupContainerServices.AddSingleton(configuration);
                    startupContainerServices.AddTypeHandler<AppSettingsTypeHandler>();
                })
                .AddStartupContainerAction((_, services) => {
                    services.AddSingleton(configuration);
                    services.AddOptions();
                });
        }

        public static BellightCoreOptions AddBellightConfigurations(this BellightCoreOptions options, bool isDevelopment = false, string? environment = null, string[]? args = null)
        {
            var configuration = new ConfigurationBuilder()
                .InitialiseBellightConfigurations(isDevelopment, environment, args)
                .Build();

            return options.AddConfigurations(configuration);
        }
    }
}
