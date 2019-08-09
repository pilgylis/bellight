using Amqp;
using Bellight.MessageBus.Abstractions;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace Bellight.MessageBus.Amqp
{
    public class AmqpPublisher : IPublisher
    {
        private readonly AmqpSenderLinkWrapper _link;
        public AmqpPublisher(IOptionsMonitor<AmqpOptions> options, string topic, MessageBusType messageBusType) {
            _link = new AmqpSenderLinkWrapper(options, topic, messageBusType);
        }

        public void Dispose()
        {
            _link?.Dispose();
        }

        public void Send(string message)
        {
            _link.GetLink().Send(new Message(message));
        }

        public Task SendAsync(string message)
        {
            return _link.GetLink().SendAsync(new Message(message));
        }
    }
}
