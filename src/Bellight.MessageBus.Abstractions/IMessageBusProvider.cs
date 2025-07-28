namespace Bellight.MessageBus.Abstractions;

public interface IMessageBusProvider
{
    IPublisher GetPublisher(string topic);

    ISubscription Subscribe(string topic, Func<string, Task> messageReceivedAction);
}