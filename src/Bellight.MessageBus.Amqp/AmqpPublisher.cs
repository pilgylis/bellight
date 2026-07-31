using Amqp;
using Bellight.MessageBus.Abstractions;

namespace Bellight.MessageBus.Amqp;

public class AmqpPublisher(
    IAmqpConnectionFactory connectionFactory,
    PublisherOptions options) : AmqpLinkWrapper<SenderLink>(connectionFactory, $"{options.MessageBusType}:{options.Topic}"), IPublisher
{
    public void Send(string message)
    {
        var link = GetLink();
        try
        {
            link.Send(new Message(message));
        }
        catch
        {
            Invalidate(link);
            throw;
        }
    }

    public async Task SendAsync(string message)
    {
        var link = GetLink();
        try
        {
            await link.SendAsync(new Message(message)).ConfigureAwait(false);
        }
        catch
        {
            Invalidate(link);
            throw;
        }
    }

    protected override SenderLink InitialiseLink(Session session)
    {
        var address = options.Address;
        return new SenderLink(session, address, address);
    }
}