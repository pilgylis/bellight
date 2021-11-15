using AutoMapper;
using Bellight.AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace Bellight.Core
{
    public static class BellightCoreOptionExtenstions
    {
        public static BellightCoreOptions AddAutoMapper(this BellightCoreOptions options, Action<IMapperConfigurationExpression>? configAction = null)
        {
            return options
                .AddStartupServiceAction(startupContainerServices => {
                    startupContainerServices.AddTypeHandler<ModelMappingTypeHandler>();
                    startupContainerServices.AddSingleton<IModelRegistrationService, DefaultModelRegistrationService>();
                })
                .AddStartupContainerAction((startupServiceProvider, services) => {
                    var modelRegistrationService = startupServiceProvider.GetService<IModelRegistrationService>();

                    services.AddSingleton<IModelMappingService>(new ModelMappingService(modelRegistrationService!, configAction!));
                });
        }
    }
}
