using Bellight.MessageBus.Abstractions;
using Bellight.MessageBus.Amqp;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static MessageBusBuilder AddAmqp(this MessageBusBuilder builder, Action<AmqpOptions> configureOption)
        {
            builder.Services.Configure(configureOption);
            builder.AddPubsubProvider<AmqpPubsubProvider>()
                .AddQueueProvider<AmqpQueueProvider>();
            return builder;
        }
    }
}
