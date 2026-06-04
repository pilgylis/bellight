using Bellight.MessageBus.Abstractions;
using Bellight.MessageBus.Amqp;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static MessageBusBuilder AddAmqp(this MessageBusBuilder builder, Action<AmqpOptions> configureOption,
        Action<AmqpAddressBuilders>? configureAddressBuilders = null)
    {
        builder.Services.Configure(configureOption);
        builder.AddPubsubProvider<AmqpPubsubProvider>()
            .AddQueueProvider<AmqpQueueProvider>();
        builder.Services.AddSingleton<IAmqpConnectionFactory, AmqpConnectionFactory>();

        var addressBuilders = new AmqpAddressBuilders();
        configureAddressBuilders?.Invoke(addressBuilders);
        builder.Services.AddSingleton(addressBuilders);

        return builder;
    }
}