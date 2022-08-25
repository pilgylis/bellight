namespace Bellight.MessageBus.Abstractions
{
    public interface ISubscriber : IDisposable
    {
        ISubscription Subscribe(Action<string> messageReceivedAction);
    }
}