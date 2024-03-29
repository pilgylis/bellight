using Bellight.Core.DependencyCache;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Bellight.Core;

public class BellightCoreOptions
{
    internal BellightCoreOptions()
    { }

    /// <summary>
    /// Location of the cache file. Default: current directory
    /// </summary>
    public DependencyCacheOptions DependencyCacheOptions { get; set; } = new DependencyCacheOptions
    {
        Enabled = true,
        PrettyPrint = false
    };

    public IList<ITypeHandler> TypeHandlers { get; set; } = new List<ITypeHandler>();
    public IList<Assembly> AdditionalAssemblies { get; set; } = new List<Assembly>();
    public IList<Action<IServiceCollection>> StartupBuilderActions { get; set; } = new List<Action<IServiceCollection>>();

    public Action<ILoggingBuilder>? LoggingBuilder { get; set; }

    public IList<Action<IServiceProvider, IServiceCollection>> StartupContainerActions { get; set; }
        = new List<Action<IServiceProvider, IServiceCollection>>();

    public BellightCoreOptions AddStartupServiceAction(Action<IServiceCollection> action)
    {
        StartupBuilderActions.Add(action);
        return this;
    }

    public BellightCoreOptions AddStartupContainerAction(Action<IServiceProvider, IServiceCollection> action)
    {
        StartupContainerActions.Add(action);
        return this;
    }
}