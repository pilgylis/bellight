using Amqp;
using Amqp.Framing;

namespace Bellight.MessageBus.Amqp.UnitTests;

/// <summary>
/// Minimal <see cref="ILink"/> test double. Real links (<see cref="SenderLink"/>/
/// <see cref="ReceiverLink"/>) are concrete classes tied directly to a live socket,
/// so they can't be exercised in a unit test without a broker - this fake stands in
/// for them wherever only the <see cref="ILink"/>/<see cref="IAmqpObject"/> contract
/// (IsClosed / Close) matters, which is all <see cref="AmqpLinkWrapper{T}"/> depends on.
/// </summary>
public class FakeLink : ILink
{
    public int CloseCallCount { get; private set; }

    public bool IsClosed { get; private set; }

    public Error Error => null!;

    public event ClosedCallback? Closed;

    public string Name => "fake-link";

    public OnLinkStateProperties? OnLinkStateProperties { get; set; }

    public void AddClosedCallback(ClosedCallback callback) => Closed += callback;

    public void Close()
    {
        CloseCallCount++;
        IsClosed = true;
        Closed?.Invoke(this, Error);
    }

    public void Close(TimeSpan waitUntilEnded, Error error) => Close();

    public Task CloseAsync()
    {
        Close();
        return Task.CompletedTask;
    }

    public Task CloseAsync(TimeSpan timeout, Error error)
    {
        Close();
        return Task.CompletedTask;
    }

    public void Detach(Error error) { }

    public Task DetachAsync(Error error) => Task.CompletedTask;
}
