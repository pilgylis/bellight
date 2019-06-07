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

            if (options.StartupBuilderActions?.Any() == true) {
                foreach (var action in options.StartupBuilderActions)
                {
                    action(startupContainerServices);
                }
            }

            var startupServiceProvider = startupContainerServices.BuildServiceProvider();

            var assemblyScanner = startupServiceProvider.GetService<IAssemblyScanner>();
            assemblyScanner.Scan(options.AdditionalAssemblies);

            if (options.StartupContainerActions?.Any() == true)
            {
                foreach (var action in options.StartupContainerActions)
                {
                    action(startupServiceProvider, services);
                }
            }

            return services;
        }
    }
}