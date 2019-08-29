using Amqp;
using Amqp.Framing;
using Amqp.Types;
using Bellight.MessageBus.Abstractions;
using System.Threading.Tasks;

namespace Bellight.MessageBus.Amqp
{
    public class AmqpPublisher : AmqpLinkWrapper<SenderLink>, IPublisher
    {
        private const string _linkName = "sender-link";
        private readonly string _topic;
        private readonly MessageBusType _messageBusType;

        public AmqpPublisher(string endpoint, string topic, MessageBusType messageBusType)
            :base(endpoint)
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
