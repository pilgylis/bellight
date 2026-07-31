using Amqp;

namespace Bellight.MessageBus.Amqp.UnitTests;

/// <summary>
/// Exposes <see cref="AmqpLinkWrapper{T}"/>'s protected members for testing, and lets a
/// test control exactly what <see cref="FakeLink"/> instance <c>InitialiseLink</c> hands
/// back next, without needing a real <see cref="Session"/>.
/// </summary>
public class TestLinkWrapper(IAmqpConnectionFactory connectionFactory, Func<FakeLink> nextLink, string sessionKey = "test-session")
    : AmqpLinkWrapper<FakeLink>(connectionFactory, sessionKey)
{
    public int InitialiseLinkCallCount { get; private set; }

    protected override FakeLink InitialiseLink(Session session)
    {
        InitialiseLinkCallCount++;
        return nextLink();
    }

    public void InvalidatePublic(FakeLink failedLink) => Invalidate(failedLink);
}
