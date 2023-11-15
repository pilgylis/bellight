using Bellight.Core;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.DependencyInjection;

namespace Bellight.MediatR;

public static class BellightCoreOptionExtensions
{
    public static BellightCoreOptions AddMediatR(this BellightCoreOptions options)
    {
        return options.AddStartupServiceAction(startupContainerServices =>
        {
            startupContainerServices.AddTypeHandler<MediatRTypeHandler>();
        }).AddStartupContainerAction((_, services) =>
        {
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestPreProcessorBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestPostProcessorBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestExceptionActionProcessorBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestExceptionProcessorBehavior<,>));
            services.AddTransient<IMediator, Mediator>();
        });
    }
}