using Bellight.Core.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bellight.MessageBus.Abstractions
{
    public class MessageBusFactory : IMessageBusFactory
    {
        private readonly string _queueConfigSection = "Providers:MessageBusQueue";
        private readonly string _pubsubConfigSection = "Providers:MessageBusPubsub";

        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MessageBusFactory> logger;

        private readonly Dictionary<MessageBusKey, IPublisher> publisherDictionary = new();
        private readonly SemaphoreSlim semaphore = new(1);

        public MessageBusFactory(IServiceProvider serviceProvider, ILogger<MessageBusFactory> logger)
        {
            _serviceProvider = serviceProvider;
            this.logger = logger;
        }

        public IPublisher GetPublisher(string topic, MessageBusType messageBusType = MessageBusType.Queue)
        {
            semaphore.Wait();
            var key = new MessageBusKey
            {
                Topic = topic,
                MessageBusType = messageBusType
            };

            if (publisherDictionary.TryGetValue(key, out var existingPublisher))
            {
                semaphore.Release();
                return existingPublisher;
            }

            var provider = GetProvider(messageBusType);
            var messageBusTypeText = messageBusType == MessageBusType.Queue ? "Queue" : "Pub/Sub";
            logger.LogInformation("MessageBus - Publisher provider created: {messageBusTypeText} - {name}", messageBusTypeText, provider?.GetType().Name);
            var publisher = provider!.GetPublisher(topic);

            if (publisher is null)
            {
                throw new MessageBusException($"Could not create publisher for {topic} - {messageBusType}");
            }

            publisherDictionary.TryAdd(key, publisher);

            semaphore.Release();
            return publisher;
        }

        public ISubscription Subscribe(string topic, Action<string> messageReceivedAction, MessageBusType messageBusType = MessageBusType.Queue)
        {
            var provider = GetProvider(messageBusType);
            var messageBusTypeText = messageBusType == MessageBusType.Queue ? "Queue" : "Pub/Sub";
            logger.LogInformation("MessageBus - Subscriber provider created: {messageBusTypeText} - {name}", messageBusTypeText, provider?.GetType().Name);
            return provider!.Subscribe(topic, messageReceivedAction);
        }

        private IMessageBusProvider? GetProvider(MessageBusType messageBusType)
        {
            try
            {
                var provider = GetProviderFromDi(messageBusType);
                return provider ?? GetProviderFromConfig(messageBusType);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error has occurred while trying to retrieve provider for Message Bus: {message}", ex.Message);
                return GetProviderFromConfig(messageBusType);
            }
        }

        private IMessageBusProvider? GetProviderFromConfig(MessageBusType messageBusType)
        {
            var configurationKey = messageBusType == MessageBusType.Queue ?
                _queueConfigSection : _pubsubConfigSection;

            var configuration = _serviceProvider.GetService<IConfiguration>();

            var messageBusTypeText = messageBusType == MessageBusType.Queue ? "Queue" : "Pub/Sub";
            if (configuration == null)
            {
                throw new ProviderNotFoundException($"MessageBus - Provider for {messageBusTypeText} must present at {configurationKey}.");
            }

            var providerTypeName = configuration[configurationKey];
            if (string.IsNullOrEmpty(providerTypeName))
            {
                throw new ProviderNotFoundException($"MessageBus - Provider for {messageBusTypeText} must present at {configurationKey}.");
            }

            var type = Type.GetType(providerTypeName);

            return _serviceProvider.GetService(type!) as IMessageBusProvider;
        }

        private IMessageBusProvider? GetProviderFromDi(MessageBusType messageBusType)
        {
            return messageBusType == MessageBusType.Queue ?
                (IMessageBusProvider?)_serviceProvider.GetService<IQueueProvider>()
                : _serviceProvider.GetService<IPubsubProvider>()!;
        }
    }
}