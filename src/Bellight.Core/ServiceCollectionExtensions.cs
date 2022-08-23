using Bellight.Core;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBellightCore(this IServiceCollection services)
    {
        return services.AddBellightCore(Enumerable.Empty<Assembly>());
    }

    public static IServiceCollection AddBellightCore(this IServiceCollection services, IEnumerable<Assembly> assemblies)
    {
        return services.AddBellightCore(config => config.AdditionalAssemblies = config.AdditionalAssemblies.Union(assemblies).ToList());
    }

    public static IServiceCollection AddBellightCore(this IServiceCollection services, Action<BellightCoreOptions> setupAction)
    {
        var options = new BellightCoreOptions();
        options.AddDependencyHandler();

        setupAction(options);

        Starter.ScanAndRegisterServices(services, options);

        return services;
    }

    public static IServiceCollection AddTypeHandler<T>(this IServiceCollection services) where T : class, ITypeHandler
    {
        services.AddScoped<T>();
        services.AddScoped<ITypeHandler, T>();
        return services;
    }
}
