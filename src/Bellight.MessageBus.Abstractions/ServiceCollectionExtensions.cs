using Bellight.MessageBus.Abstractions;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static MessageBusBuilder AddBellightMessageBus(this IServiceCollection services)
    {
        services.AddSingleton<IMessageBusFactory, MessageBusFactory>();
        return new MessageBusBuilder(services);
    }
}