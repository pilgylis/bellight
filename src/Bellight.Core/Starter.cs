using System;
using System.Collections.Generic;
using System.Linq;
using Bellight.Core.Defaults;
using Microsoft.Extensions.DependencyInjection;
namespace Bellight.Core 
{
    internal static class Starter
    {
        public static IServiceCollection ScanAndRegisterServices(IServiceCollection services, BellightCoreOptions options)
        {
            var startupContainerServices = new ServiceCollection();

            startupContainerServices.AddTransient<IAssemblyLoader, DefaultAssemblyLoader>();
            startupContainerServices.AddTransient<IAssemblyHandler, DefaultAssemblyHandler>();
            startupContainerServices.AddTransient<IAssemblyScanner, DefaultAssemblyScanner>();

            startupContainerServices.AddSingleton(services);
            IDictionary<string, Type> keyedTypeDictionary = new Dictionary<string, Type>();
            startupContainerServices.AddSingleton(keyedTypeDictionary);
            startupContainerServices.AddTransient<ITypeHandler, DependencyTypeHandler>();

            if (options.StartupBuilderActions?.Any() == true) {
                foreach (var action in options.StartupBuilderActions)
                {
                    action(startupContainerServices);
                }
            }

            var startupContainer = startupContainerServices.BuildServiceProvider();

            var assemblyScanner = startupContainer.GetService<IAssemblyScanner>();
            assemblyScanner.Scan(options.AdditionalAssemblies);

            if (options.StartupContainerActions?.Any() == true)
            {
                foreach (var action in options.StartupContainerActions)
                {
                    action(startupContainer);
                }
            }

            services.AddScoped<IKeyedServiceFactory>(sp => new KeyedServiceFactory(keyedTypeDictionary, sp));

            return services;
        }
    }
}