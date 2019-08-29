using Bellight.MessageBus.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;

namespace Bellight.MessageBus.Amqp
{
    public abstract class AmqpProviderBase
    {
        private readonly IConfiguration _configuration;
        private readonly MessageBusType _messageBusType;
        private readonly IOptionsMonitor<AmqpOptions> _options;

        public AmqpProviderBase(
            IConfiguration configuration,
            IOptionsMonitor<AmqpOptions> options,
            MessageBusType messageBusType)
        {
            _configuration = configuration;
            _messageBusType = messageBusType;
            _options = options;
        }

        public IPublisher GetPublisher(string topic)
        {
            var endpoint = _configuration[Constants.EndpointConfig];
            return new AmqpPublisher(endpoint, topic, _messageBusType);
        }

        public ISubscription Subscribe(string topic, Action<string> messageReceivedAction)
        {
            var endpoint = _configuration[Constants.EndpointConfig];
            var options = _options.CurrentValue;
            var subscriberOptions = new SubscriberOptions
            {
                Endpoint = endpoint,
                Topic = topic,
                MessageBusType = _messageBusType,
                PollingInterval = options.PollingIntervalMilliseconds,
                WaitDuration = options.WaitDurationMilliseconds
            };

            var subscriber = new AmqpSubscriber(subscriberOptions);

            return subscriber.Subscribe(messageReceivedAction);
        }
    }
}
