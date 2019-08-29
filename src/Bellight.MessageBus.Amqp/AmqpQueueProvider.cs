using Bellight.MessageBus.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Bellight.MessageBus.Amqp
{
    public class AmqpQueueProvider : AmqpProviderBase, IQueueProvider
    {
        public AmqpQueueProvider(IConfiguration configuration, IOptionsMonitor<AmqpOptions> options)
            : base(configuration, options, MessageBusType.Queue)
        {
        }
    }
}
