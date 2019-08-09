using System;

namespace Bellight.MessageBus.Abstractions
{
    public interface ISubscriber: IDisposable
    {
        ISubscription Subscribe(string topic, Action<string> messageReceivedAction, MessageBusType messageBusType = MessageBusType.Queue);
    }
}
