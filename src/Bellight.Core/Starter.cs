using System;
using System.Linq;
using System.Threading.Tasks;
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


            startupContainerServices.AddSingleton(options);

            startupContainerServices.AddScoped<IDependencyCacheService, DefaultDependencyCacheService>();
            startupContainerServices.AddScoped<IServiceCollection, ServiceCollection>();

            if (options.StartupBuilderActions?.Any() == true) {
                foreach (var action in options.StartupBuilderActions)
                {
                    action(startupContainerServices);
                }
            }

            var startupServiceProvider = startupContainerServices.BuildServiceProvider();


            IServiceCollection? innerServices = null;
            using (var cacheScope = startupServiceProvider.CreateScope())
            {
                var dependencyCacheService = cacheScope.ServiceProvider.GetRequiredService<IDependencyCacheService>();

                if (dependencyCacheService.Load())
                {
                    innerServices = cacheScope.ServiceProvider.GetRequiredService<IServiceCollection>();
                }
            }

            if (innerServices != null)
            {
                MergeServiceCollections(services, innerServices);
            }
            else
            {
                using (var loadScope = startupServiceProvider.CreateScope())
                {
                    var assemblyScanner = loadScope.ServiceProvider.GetRequiredService<IAssemblyScanner>();
                    var dependencyModel = assemblyScanner.Scan();
                    innerServices = loadScope.ServiceProvider.GetRequiredService<IServiceCollection>();
                    MergeServiceCollections(services, innerServices);

                    Task.Factory.StartNew(() => SaveDependencyCache(dependencyModel, startupServiceProvider));
                }
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

        private static void SaveDependencyCache(DependencyCacheModel model, IServiceProvider rootServiceProvider)
        {
            using (var scope = rootServiceProvider.CreateScope())
            {
                var dependencyCacheService = scope.ServiceProvider.GetRequiredService<IDependencyCacheService>();
                dependencyCacheService.Save(model);
            }
        }

        private static void MergeServiceCollections(IServiceCollection target, IServiceCollection source)
        {
            foreach (var descriptor in source)
            {
                target.Add(descriptor);
            }
        }
    }
}