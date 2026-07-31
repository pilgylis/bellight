using Amqp;
using Bellight.MessageBus.Abstractions;

namespace Bellight.MessageBus.Amqp;

public class AmqpPublisher(
    IAmqpConnectionFactory connectionFactory,
    PublisherOptions options) : AmqpLinkWrapper<SenderLink>(connectionFactory), IPublisher
{
    public void Send(string message)
    {
        try
        {
            GetLink().Send(new Message(message));
        }
        catch
        {
            Invalidate();
            throw;
        }
    }

    public async Task SendAsync(string message)
    {
        try
        {
            await GetLink().SendAsync(new Message(message)).ConfigureAwait(false);
        }
        catch
        {
            Invalidate();
            throw;
        }
    }

    protected override SenderLink InitialiseLink(Session session)
    {
        var address = options.Address;
        return new SenderLink(session, address, address);
    }
}