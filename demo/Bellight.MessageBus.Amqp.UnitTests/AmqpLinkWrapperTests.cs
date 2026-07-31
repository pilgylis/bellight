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
    public void Invalidate_ClosesTheCurrentLink_AndResetsTheConnectionFactory()
    {
        var link = new FakeLink();
        var wrapper = new TestLinkWrapper(_connectionFactory.Object, () => link);
        wrapper.GetLink();

        wrapper.InvalidatePublic();

        Assert.Equal(1, link.CloseCallCount);
        Assert.True(link.IsClosed);
        _connectionFactory.Verify(f => f.Reset(), Times.Once);
    }

    [Fact]
    public void Invalidate_DoesNotCloseTheLinkAgain_WhenItIsAlreadyClosed()
    {
        var link = new FakeLink();
        var wrapper = new TestLinkWrapper(_connectionFactory.Object, () => link);
        wrapper.GetLink();
        link.Close();

        wrapper.InvalidatePublic();

        Assert.Equal(1, link.CloseCallCount);
        _connectionFactory.Verify(f => f.Reset(), Times.Once);
    }

    [Fact]
    public void Invalidate_ResetsTheConnectionFactory_EvenWhenNoLinkWasEverCreated()
    {
        var wrapper = new TestLinkWrapper(_connectionFactory.Object, () => new FakeLink());

        var exception = Record.Exception(() => wrapper.InvalidatePublic());

        Assert.Null(exception);
        _connectionFactory.Verify(f => f.Reset(), Times.Once);
    }

    [Fact]
    public void GetLink_EstablishesANewLink_AfterInvalidate()
    {
        var links = new Queue<FakeLink>([new FakeLink(), new FakeLink()]);
        var wrapper = new TestLinkWrapper(_connectionFactory.Object, links.Dequeue);

        var first = wrapper.GetLink();
        wrapper.InvalidatePublic();
        var second = wrapper.GetLink();

        Assert.NotSame(first, second);
        Assert.Equal(2, wrapper.InitialiseLinkCallCount);
    }
}
