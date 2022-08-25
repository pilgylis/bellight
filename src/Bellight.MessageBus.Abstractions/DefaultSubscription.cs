namespace Bellight.MessageBus.Abstractions
{
    public class DefaultSubscription : ISubscription
    {
        private readonly Action _disposeAction;

        public DefaultSubscription(Action disposeAction)
        {
            _disposeAction = disposeAction ?? throw new ArgumentNullException(nameof(disposeAction));
        }

        public virtual void Dispose()
        {
            _disposeAction.Invoke();
        }
    }
}