namespace Bellight.MessageBus.Abstractions;

public sealed class DefaultSubscription(CancellationTokenSource tokenSource) : ISubscription
{
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        tokenSource?.Cancel();
        tokenSource?.Dispose();
    }
}