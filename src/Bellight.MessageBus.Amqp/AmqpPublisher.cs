using Amqp;
using Bellight.MessageBus.Abstractions;

namespace Bellight.MessageBus.Amqp;

public class AmqpPublisher(
    IAmqpConnectionFactory connectionFactory,
    PublisherOptions options) : AmqpLinkWrapper<SenderLink>(connectionFactory), IPublisher
{
    public void Send(string message)
    {
        GetLink().Send(new Message(message));
    }

    public Task SendAsync(string message)
    {
        return GetLink().SendAsync(new Message(message));
    }

    protected override SenderLink InitialiseLink(Session session)
    {
        var address = options.Address;
        return new SenderLink(session, address, address);
    }
}