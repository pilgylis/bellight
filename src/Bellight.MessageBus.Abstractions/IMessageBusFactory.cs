namespace Bellight.MessageBus.Abstractions;

public interface IMessageBusFactory
{
    IPublisher GetPublisher(string topic, MessageBusType messageBusType = MessageBusType.Queue);

    ISubscription Subscribe(string topic, Func<string, Task> messageReceivedAction, MessageBusType messageBusType = MessageBusType.Queue);
}