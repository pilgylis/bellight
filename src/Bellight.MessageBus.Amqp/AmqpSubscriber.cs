using System;
using System.Collections.Concurrent;
using Bellight.MessageBus.Abstractions;
using Microsoft.Extensions.Options;

namespace Bellight.MessageBus.Amqp
{
    public class AmqpSubscriber: ISubscriber
    {
        private readonly ConcurrentDictionary<string, AmqpReceiverLinkWrapper> _linkDictionary 
            = new ConcurrentDictionary<string, AmqpReceiverLinkWrapper>();
        private readonly IOptionsMonitor<AmqpOptions> _options;

        public AmqpSubscriber(IOptionsMonitor<AmqpOptions> options) {
            _options = options;
        }

        public ISubscription Subscribe(string topic, Action<string> messageReceivedAction, MessageBusType messageBusType = MessageBusType.Queue)
        {
            string subscriptionId;
            do
            {
                subscriptionId = Guid.NewGuid().ToString();
            } while (_linkDictionary.ContainsKey(subscriptionId));

            var link = new AmqpReceiverLinkWrapper(
                topic,
                _options,
                messageReceivedAction,
                messageBusType);

            _linkDictionary.TryAdd(subscriptionId, link);

            return new DefaultSubscription(() => Unsubscribe(subscriptionId));
        }

        private void Unsubscribe(string subscriptionId)
        {
            _linkDictionary.TryRemove(subscriptionId, out var link);
            link?.Dispose();
        }

        public void Dispose()
        {
            if (_linkDictionary.Count == 0)
            {
                return;
            }

            foreach (var link in _linkDictionary.Values)
            {
                link?.Dispose();
            }
        }
    }
}
