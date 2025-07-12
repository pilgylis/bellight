using Bellight.Core;
using MediatR.Registration;
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
            var mediatRServiceConfiguration = new MediatRServiceConfiguration();
            mediatRConfigure(mediatRServiceConfiguration);
            ServiceRegistrar.SetGenericRequestHandlerRegistrationLimitations(mediatRServiceConfiguration);
            ServiceRegistrar.AddMediatRClassesWithTimeout(services, mediatRServiceConfiguration);
            ServiceRegistrar.AddRequiredServices(services, mediatRServiceConfiguration);
        });
    }
}
