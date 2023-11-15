namespace Bellight.MessageBus.Abstractions;

public class DefaultSubscription(Action disposeAction) : ISubscription
{
    private readonly Action _disposeAction = disposeAction ?? throw new ArgumentNullException(nameof(disposeAction));

    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
        _disposeAction.Invoke();
    }
}