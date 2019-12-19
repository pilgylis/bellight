using Bellight.MessageBus.Abstractions;
using Microsoft.Extensions.Options;

namespace Bellight.MessageBus.Amqp
{
    public class AmqpQueueProvider : AmqpProviderBase, IQueueProvider
    {
        public AmqpQueueProvider(IOptionsMonitor<AmqpOptions> options)
            : base(options, MessageBusType.Queue)
        {
        }
    }
}
