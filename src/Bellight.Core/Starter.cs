using System;
using System.Collections.Generic;
using System.Linq;
using Bellight.Core.Defaults;
using Bellight.Core.DependencyCache;
using Bellight.Core.Misc;
using Microsoft.Extensions.DependencyInjection;
namespace Bellight.Core
{
    internal static class Starter
    {
        public static IServiceCollection ScanAndRegisterServices(IServiceCollection services, BellightCoreOptions options)
        {
            var startupContainerServices = new ServiceCollection();

            startupContainerServices.AddTransient<ISerializer, BellightJsonSerializer>();
            startupContainerServices.AddTransient<IAssemblyLoader, DefaultAssemblyLoader>();
            startupContainerServices.AddTransient<IAssemblyHandler, DefaultAssemblyHandler>();
            startupContainerServices.AddTransient<IAssemblyScanner, DefaultAssemblyScanner>();

            startupContainerServices.AddSingleton<IDependencyCacheService, DefaultDependencyCacheService>();

            startupContainerServices.AddSingleton(services);
            startupContainerServices.AddSingleton(options);

            if (options.StartupBuilderActions?.Any() == true) {
                foreach (var action in options.StartupBuilderActions)
                {
                    action(startupContainerServices);
                }
            }

            var startupServiceProvider = startupContainerServices.BuildServiceProvider();

            var dependencyCacheService = startupServiceProvider.GetService<IDependencyCacheService>();
            var dependencyModel = dependencyCacheService.Load();
            if (dependencyModel == null)
            {
                var assemblyScanner = startupServiceProvider.GetService<IAssemblyScanner>();
                dependencyModel = assemblyScanner.Scan();

                dependencyCacheService.Save(dependencyModel);
            }

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