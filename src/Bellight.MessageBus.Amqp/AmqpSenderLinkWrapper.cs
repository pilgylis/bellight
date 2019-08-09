using Amqp;
using Amqp.Framing;
using Amqp.Types;
using Bellight.MessageBus.Abstractions;
using Microsoft.Extensions.Options;

namespace Bellight.MessageBus.Amqp
{
    public class AmqpSenderLinkWrapper : AmqpLinkWrapper<SenderLink>
    {
        private const string _linkName = "sender-link";
        private readonly string _topic;
        private readonly MessageBusType _messageBusType;

        public AmqpSenderLinkWrapper(IOptionsMonitor<AmqpOptions> options, string topic, MessageBusType messageBusType) : base(options)
        {
            _topic = topic;
            _messageBusType = messageBusType;
        }

        protected override SenderLink InitialiseLink(Session session)
        {
            var target = new Target
            {
                Address = _topic,
                Capabilities = new Symbol[] {
                    new Symbol(_messageBusType == MessageBusType.Queue ? "queue" : "topic")
                }
            };

            return new SenderLink(session, _linkName, target, null);
        }
    }
}
