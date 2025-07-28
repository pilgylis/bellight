namespace Bellight.MessageBus.Abstractions;

public interface ISubscriber : IDisposable
{
    ISubscription Subscribe(Func<string, Task> messageReceivedAction);
}