using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Bellight.Core
{
    public class BellightCoreOptions
    {
        internal BellightCoreOptions() {}

        public IList<ITypeHandler> TypeHandlers { get; set; } = new List<ITypeHandler>();
        public IList<Assembly> AdditionalAssemblies { get; set; } = new List<Assembly>();
        public IList<Action<IServiceCollection>> StartupBuilderActions { get; set; } = new List<Action<IServiceCollection>>();
        public IList<Action<IServiceProvider>> StartupContainerActions { get; set; } = new List<Action<IServiceProvider>>();

        public BellightCoreOptions AddStartupServiceAction(Action<IServiceCollection> action)
        {
            StartupBuilderActions.Add(action);
            return this;
        }

        public BellightCoreOptions AddStartupContainerAction(Action<IServiceProvider> action)
        {
            StartupContainerActions.Add(action);
            return this;
        }
    }
}
