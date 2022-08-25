using Amqp;
using Amqp.Framing;
using Amqp.Types;
using Bellight.MessageBus.Abstractions;

namespace Bellight.MessageBus.Amqp;

public class AmqpPublisher : AmqpLinkWrapper<SenderLink>, IPublisher
{
    private readonly string _topic;
    private readonly MessageBusType _messageBusType;

    public AmqpPublisher(IAmqpConnectionFactory connectionFactory, string topic, MessageBusType messageBusType)
        : base(connectionFactory)
    {
        _topic = topic;
        _messageBusType = messageBusType;
    }

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
        var messageBusTypeText = _messageBusType == MessageBusType.Queue ? "queue" : "topic";
        var target = new Target
        {
            Address = _topic,
            Capabilities = new Symbol[] {
                new Symbol(messageBusTypeText)
            }
        };

        var linkName = $"{messageBusTypeText}.{_topic}";

        return new SenderLink(session, linkName, target, null);
    }
}