using Bellight.MessageBus.Abstractions;
using Microsoft.Extensions.Options;

namespace Bellight.MessageBus.Amqp;

public class AmqpQueueProvider : AmqpProviderBase, IQueueProvider
{
    public AmqpQueueProvider(IAmqpConnectionFactory connectionFactory, IOptionsMonitor<AmqpOptions> options)
        : base(connectionFactory, options, MessageBusType.Queue)
    {
    }
}
