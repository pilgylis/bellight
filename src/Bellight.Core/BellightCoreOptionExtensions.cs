using Bellight.Core.Defaults;
using Microsoft.Extensions.DependencyInjection;

namespace Bellight.Core;

public static class BellightCoreOptionExtensions
{
    public static BellightCoreOptions AddDependencyHandler(this BellightCoreOptions options)
    {
        return options
            .AddStartupServiceAction(startupContainerServices =>
            {
                startupContainerServices.AddSingleton<IKeyedServiceRegistry, DefaultKeyedServiceRegistry>();
                startupContainerServices.AddScoped<DependencyTypeHandler>();
                startupContainerServices.AddScoped<ITypeHandler, DependencyTypeHandler>();
            })
            .AddStartupContainerAction((startupServiceProvider, services) =>
            {
                var keyedServiceRegistry = startupServiceProvider.GetRequiredService<IKeyedServiceRegistry>();
                var keyedTypeDictionary = keyedServiceRegistry.GetDictionary();
                services.AddScoped<IKeyedServiceFactory>(sp => new KeyedServiceFactory(keyedTypeDictionary, sp));
            });
    }
}