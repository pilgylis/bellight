using Bellight.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Bellight.MediatR;

public static class BellightCoreOptionExtensions
{
    public static BellightCoreOptions AddMediatR(this BellightCoreOptions options, Action<MediatRServiceConfiguration> mediatRConfigure)
    {
        return options.AddStartupServiceAction(startupContainerServices =>
        {
            startupContainerServices.AddTypeHandler<MediatRTypeHandler>();
        }).AddStartupContainerAction((_, services) =>
        {
            services.AddMediatR(cfg =>
            {
                mediatRConfigure.Invoke(cfg);
            });
        });
    }
}
