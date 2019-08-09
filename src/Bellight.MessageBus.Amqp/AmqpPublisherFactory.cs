using Bellight.MessageBus.Abstractions;
using Microsoft.Extensions.Options;

namespace Bellight.MessageBus.Amqp
{
    public class AmqpPublisherFactory : IPublisherFactory
    {
        private readonly IOptionsMonitor<AmqpOptions> _options;

        public AmqpPublisherFactory(IOptionsMonitor<AmqpOptions> options) {
            _options = options;
        }

        public IPublisher GetPublisher(string topic, MessageBusType messageBusType = MessageBusType.Queue)
        {
            return new AmqpPublisher(_options, topic, messageBusType);
        }
    }
}
