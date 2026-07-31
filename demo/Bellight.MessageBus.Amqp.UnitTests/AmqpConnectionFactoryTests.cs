using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Bellight.MessageBus.Amqp.UnitTests;

public class AmqpConnectionFactoryTests
{
    // AmqpConnectionFactory.GetSession() opens a real socket via the AMQPNetLite
    // Connection/Session constructors, so exercising the connected path (an established
    // connection/session actually getting torn down by Reset()) needs a live broker and is
    // out of scope for a unit test. This covers the safe-no-op path that doesn't require
    // one: Reset() must never throw for a name that was never established - e.g. a link
    // operation that fails before its connection/session was ever created.
    [Fact]
    public void Reset_DoesNotThrow_ForASessionThatWasNeverCreated()
    {
        var options = new Mock<IOptionsMonitor<AmqpOptions>>();
        options.Setup(o => o.CurrentValue).Returns(new AmqpOptions { Endpoint = "amqp://localhost:5672" });
        var factory = new AmqpConnectionFactory(options.Object);

        var exception = Record.Exception(() => factory.Reset("never-created"));

        Assert.Null(exception);
    }
}
