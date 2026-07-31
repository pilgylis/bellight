using Amqp;
using Moq;
using Xunit;

namespace Bellight.MessageBus.Amqp.UnitTests;

public class AmqpLinkWrapperTests
{
    private readonly Mock<IAmqpConnectionFactory> _connectionFactory = new();

    public AmqpLinkWrapperTests()
    {
        // TestLinkWrapper.InitialiseLink ignores the session it's handed, so a null
        // stand-in is fine - a real Session can't be constructed without a live connection.
        _connectionFactory.Setup(f => f.GetSession(It.IsAny<string>())).Returns((Session)null!);
    }

    [Fact]
    public void GetLink_CachesTheSameLinkAcrossCalls()
    {
        var link = new FakeLink();
        var wrapper = new TestLinkWrapper(_connectionFactory.Object, () => link);

        var first = wrapper.GetLink();
        var second = wrapper.GetLink();

        Assert.Same(first, second);
        Assert.Equal(1, wrapper.InitialiseLinkCallCount);
    }

    [Fact]
    public void GetLink_CreatesAFreshLink_WhenThePreviousOneIsClosed()
    {
        var links = new Queue<FakeLink>([new FakeLink(), new FakeLink()]);
        var wrapper = new TestLinkWrapper(_connectionFactory.Object, links.Dequeue);

        var first = wrapper.GetLink();
        first.Close();
        var second = wrapper.GetLink();

        Assert.NotSame(first, second);
        Assert.Equal(2, wrapper.InitialiseLinkCallCount);
    }

    [Fact]
    public void Invalidate_ClosesTheFailedLink_AndResetsTheConnectionFactory()
    {
        var link = new FakeLink();
        var wrapper = new TestLinkWrapper(_connectionFactory.Object, () => link);
        var current = wrapper.GetLink();

        wrapper.InvalidatePublic(current);

        Assert.Equal(1, link.CloseCallCount);
        Assert.True(link.IsClosed);
        _connectionFactory.Verify(f => f.Reset(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void Invalidate_DoesNotCloseTheLinkAgain_WhenItIsAlreadyClosed()
    {
        var link = new FakeLink();
        var wrapper = new TestLinkWrapper(_connectionFactory.Object, () => link);
        var current = wrapper.GetLink();
        link.Close();

        wrapper.InvalidatePublic(current);

        Assert.Equal(1, link.CloseCallCount);
        _connectionFactory.Verify(f => f.Reset(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void GetLink_EstablishesANewLink_AfterInvalidate()
    {
        var links = new Queue<FakeLink>([new FakeLink(), new FakeLink()]);
        var wrapper = new TestLinkWrapper(_connectionFactory.Object, links.Dequeue);

        var first = wrapper.GetLink();
        wrapper.InvalidatePublic(first);
        var second = wrapper.GetLink();

        Assert.NotSame(first, second);
        Assert.Equal(2, wrapper.InitialiseLinkCallCount);
    }

    // Regression test for a production incident: two concurrent callers on the same
    // topic each held their own reference to the link that was current when they started
    // an operation. One of them failed and called Invalidate() while the other had
    // already recovered (via its own GetLink() call) and installed a fresh link. Without
    // reference-checking, the failing caller's Invalidate() closed the *replacement* link
    // out from under the caller that was actively using it, and reset the shared
    // connection/session mid-flight - producing "has been attached by a link with the
    // same role" and "not valid under state: DetachSent" errors from concurrent Quartz
    // job dispatches sharing the cached "scheduler" topic publisher.
    [Fact]
    public void Invalidate_IsANoOp_WhenTheLinkHasAlreadyBeenReplaced()
    {
        var links = new Queue<FakeLink>([new FakeLink(), new FakeLink()]);
        var wrapper = new TestLinkWrapper(_connectionFactory.Object, links.Dequeue);

        var staleLink = wrapper.GetLink();
        staleLink.Close(); // e.g. the broker forced it closed for an unrelated reason
        var freshLink = wrapper.GetLink(); // another caller already recovered

        wrapper.InvalidatePublic(staleLink); // the original failing caller catches up late

        Assert.Same(freshLink, wrapper.GetLink());
        Assert.False(freshLink.IsClosed);
        Assert.Equal(0, freshLink.CloseCallCount);
        _connectionFactory.Verify(f => f.Reset(It.IsAny<string>()), Times.Never);
    }

    // Each publisher/subscriber now owns its own session (keyed by topic), instead of every
    // topic sharing one "default" session - a wedged link on one topic must not affect
    // another topic's session at all. Regression test for the incident where "worker" and
    // "scheduler" forced onto the same session caused a second link's attach to hang.
    [Fact]
    public void Invalidate_OnlyResetsThisWrappersOwnSessionKey()
    {
        var link = new FakeLink();
        var wrapper = new TestLinkWrapper(_connectionFactory.Object, () => link, sessionKey: "Queue:worker");
        var current = wrapper.GetLink();

        wrapper.InvalidatePublic(current);

        _connectionFactory.Verify(f => f.Reset("Queue:worker"), Times.Once);
        _connectionFactory.Verify(f => f.Reset(It.Is<string>(k => k != "Queue:worker")), Times.Never);
    }

    [Fact]
    public void GetLink_CreatesExactlyOneLink_UnderConcurrentCallers()
    {
        var wrapper = new TestLinkWrapper(_connectionFactory.Object, () => new FakeLink());

        var results = new FakeLink[50];
        Parallel.For(0, results.Length, i => results[i] = wrapper.GetLink());

        Assert.Equal(1, wrapper.InitialiseLinkCallCount);
        Assert.True(results.All(r => ReferenceEquals(r, results[0])));
    }

    [Fact]
    public void ConcurrentInvalidateAndGetLink_NeverLeaveAClosedLinkCached()
    {
        var wrapper = new TestLinkWrapper(_connectionFactory.Object, () => new FakeLink());

        Parallel.For(0, 200, i =>
        {
            var link = wrapper.GetLink();
            if (i % 2 == 0)
            {
                wrapper.InvalidatePublic(link);
            }
        });

        var final = wrapper.GetLink();
        Assert.False(final.IsClosed);
    }
}
