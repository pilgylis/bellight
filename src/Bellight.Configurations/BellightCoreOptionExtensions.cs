using Bellight.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bellight.Core
{
    public static class BellightCoreOptionExtensions
    {
        public static BellightCoreOptions AddScanConfigurations(this BellightCoreOptions options, IConfiguration configuration)
        {
            return options
                .AddStartupServiceAction(startupContainerServices => {
                    startupContainerServices.AddSingleton(configuration);
                    startupContainerServices.AddTransient<ITypeHandler, AppSettingsTypeHandler>();
                })
                .AddStartupContainerAction((_, services) => {
                    services.AddSingleton(services);
                    services.AddOptions();
                });
        }
    }
}
