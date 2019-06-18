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


            IServiceCollection innerServices = null;
            using (var cacheScope = startupServiceProvider.CreateScope())
            {
                var dependencyCacheService = cacheScope.ServiceProvider.GetService<IDependencyCacheService>();

                if (dependencyCacheService.Load())
                {
                    innerServices = cacheScope.ServiceProvider.GetService<IServiceCollection>();
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
                    var assemblyScanner = loadScope.ServiceProvider.GetService<IAssemblyScanner>();
                    var dependencyModel = assemblyScanner.Scan();
                    innerServices = loadScope.ServiceProvider.GetService<IServiceCollection>();
                    MergeServiceCollections(services, innerServices);

                    // TODO: move the save to a different thread, with its scope
                    var dependencyCacheService = loadScope.ServiceProvider.GetService<IDependencyCacheService>();
                    dependencyCacheService.Save(dependencyModel);
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

        private static void MergeServiceCollections(IServiceCollection target, IServiceCollection source)
        {
            foreach (var descriptor in source)
            {
                target.Add(descriptor);
            }
        }
    }
}