using Bellight.Core.Defaults;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Bellight.Core
{
    public static class BellightCoreOptionExtensions
    {
        public static BellightCoreOptions AddDependencyHandler(this BellightCoreOptions options)
        {
            IDictionary<string, Type> keyedTypeDictionary = new Dictionary<string, Type>();
            return options
                .AddStartupServiceAction(startupContainerServices =>
                {
                    startupContainerServices.AddSingleton(keyedTypeDictionary);
                    startupContainerServices.AddSingleton<ITypeHandler, DependencyTypeHandler>();
                })
                .AddStartupContainerAction((_, services) => {
                    services.AddScoped<IKeyedServiceFactory>(sp => new KeyedServiceFactory(keyedTypeDictionary, sp));
                });
        }
    }
}
