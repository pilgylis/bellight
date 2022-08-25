using Bellight.MessageBus.Abstractions;
using Microsoft.Extensions.Options;

namespace Bellight.MessageBus.Amqp;

public class AmqpPubsubProvider : AmqpProviderBase, IPubsubProvider
{
    public AmqpPubsubProvider(IAmqpConnectionFactory connectionFactory, IOptionsMonitor<AmqpOptions> options)
        : base(connectionFactory, options, MessageBusType.PubSub)
    {
    }
}