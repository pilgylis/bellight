using Bellight.Core.Exceptions;
using Bellight.Core.Misc;
using Microsoft.Extensions.Configuration;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace Bellight.MessageBus.Abstractions
{
    public class MessageBusFactory : IMessageBusFactory
    {
        private readonly string _queueConfigSection = "Providers:MessageBusQueue";
        private readonly string _pubsubConfigSection = "Providers:MessageBusPubsub";

        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;

        public MessageBusFactory(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
        }

        public IPublisher GetPublisher(string topic, MessageBusType messageBusType = MessageBusType.Queue)
        {
            var provider = GetProvider(messageBusType);
            var messageBusTypeText = messageBusType == MessageBusType.Queue ? "Queue" : "Pub/Sub";
            StaticLog.Information($"MessageBus - Publisher provider created: {messageBusTypeText} - {provider.GetType().Name}");
            return provider.GetPublisher(topic);
        }

        public ISubscription Subscribe(string topic, Action<string> messageReceivedAction, MessageBusType messageBusType = MessageBusType.Queue)
        {
            var provider = GetProvider(messageBusType);
            var messageBusTypeText = messageBusType == MessageBusType.Queue ? "Queue" : "Pub/Sub";
            StaticLog.Information($"MessageBus - Subscriber provider created: {messageBusTypeText} - {provider.GetType().Name}");
            return provider.Subscribe(topic, messageReceivedAction);
        }

        private IMessageBusProvider GetProvider(MessageBusType messageBusType)
        {
            var configurationKey = messageBusType == MessageBusType.Queue ?
                _queueConfigSection : _pubsubConfigSection;

            var providerTypeName = _configuration[configurationKey];
            if (string.IsNullOrEmpty(providerTypeName))
            {
                var provider = GetProviderFromDi(messageBusType);

                if (provider == null)
                {
                    var messageBusTypeText = messageBusType == MessageBusType.Queue ? "Queue" : "Pub/Sub";
                    throw new ProviderNotFoundException($"MessageBus - Provider for {messageBusTypeText} must present at {configurationKey}.");
                }
            }

            var type = Type.GetType(providerTypeName);

            return _serviceProvider.GetService(type) as IMessageBusProvider;
        }

        private IMessageBusProvider GetProviderFromDi(MessageBusType messageBusType)
        {
            return messageBusType == MessageBusType.Queue ?
                (IMessageBusProvider)_serviceProvider.GetService<IQueueProvider>()
                : _serviceProvider.GetService<IPubsubProvider>();
        }
    }
}
