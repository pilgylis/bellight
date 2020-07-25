using Bellight.MessageBus.Abstractions;
using Microsoft.Extensions.Options;
using System;

namespace Bellight.MessageBus.Amqp
{
    public abstract class AmqpProviderBase
    {
        private readonly MessageBusType _messageBusType;
        private readonly IOptionsMonitor<AmqpOptions> _options;

        public AmqpProviderBase(
            IOptionsMonitor<AmqpOptions> options,
            MessageBusType messageBusType)
        {
            _messageBusType = messageBusType;
            _options = options;
        }

        public IPublisher GetPublisher(string topic)
        {
            return new AmqpPublisher(_options.CurrentValue.Endpoint, NormalizeTopic(topic), _messageBusType);
        }

        public ISubscription Subscribe(string topic, Action<string> messageReceivedAction)
        {
            var options = _options.CurrentValue;
            var subscriberOptions = new SubscriberOptions
            {
                Endpoint = options.Endpoint,
                Topic = NormalizeTopic(topic),
                MessageBusType = _messageBusType,
                PollingInterval = options.PollingIntervalMilliseconds,
                WaitDuration = options.WaitDurationMilliseconds,
                IsAzureMessageBus = options.IsAzureMessageBus,
                SubscriberName = options.SubscriberName
            };

            var subscriber = new AmqpSubscriber(subscriberOptions);

            return subscriber.Subscribe(messageReceivedAction);
        }

        private string NormalizeTopic(string topic) => string.IsNullOrEmpty(_options.CurrentValue.InstanceName) ? 
            topic : $"{_options.CurrentValue.InstanceName}.{topic}";
    }
}
