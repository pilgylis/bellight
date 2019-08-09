using System;

namespace Bellight.MessageBus.Abstractions
{
    public interface IPublisherFactory
    {
        IPublisher GetPublisher(string topic, MessageBusType messageBusType = MessageBusType.Queue);
    }
}
