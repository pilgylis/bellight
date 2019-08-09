using Bellight.MessageBus.Abstractions;
using Bellight.MessageBus.Amqp;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAmqpMessageBus(this IServiceCollection services, Action<AmqpOptions> configureOption)
        {
            return services.Configure(configureOption)
                .AddTransient<IPublisherFactory, AmqpPublisherFactory>()
                .AddSingleton<ISubscriber, AmqpSubscriber>();
        }
    }
}
