using Amqp;
using Amqp.Framing;
using Amqp.Types;
using Bellight.MessageBus.Abstractions;

namespace Bellight.MessageBus.Amqp;

public class AmqpPublisher(
    IAmqpConnectionFactory connectionFactory, 
    string topic, 
    MessageBusType messageBusType) : AmqpLinkWrapper<SenderLink>(connectionFactory), IPublisher
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
        var messageBusTypeText = messageBusType == MessageBusType.Queue ? "queue" : "topic";
        var target = new Target
        {
            Address = topic,
            Capabilities =
            [
                new Symbol(messageBusTypeText)
            ]
        };

        var linkName = $"{messageBusTypeText}.{topic}";

        return new SenderLink(session, linkName, target, null);
    }
}