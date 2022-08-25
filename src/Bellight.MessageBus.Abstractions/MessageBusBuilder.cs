using Microsoft.Extensions.DependencyInjection;

namespace Bellight.MessageBus.Abstractions
{
    public class MessageBusBuilder
    {
        internal MessageBusBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }

        public MessageBusBuilder AddQueueProvider<T>() where T : class, IQueueProvider
        {
            Services.AddTransient<IQueueProvider, T>();
            return this;
        }

        public MessageBusBuilder AddPubsubProvider<T>() where T : class, IPubsubProvider
        {
            Services.AddTransient<IPubsubProvider, T>();
            return this;
        }
    }
}