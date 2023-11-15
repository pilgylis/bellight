using Bellight.MessageBus.Abstractions;
using Microsoft.Extensions.Options;

namespace Bellight.MessageBus.Amqp;

public class AmqpQueueProvider(
    IAmqpConnectionFactory connectionFactory, 
    IOptionsMonitor<AmqpOptions> options) : AmqpProviderBase(
        connectionFactory, 
        options, 
        MessageBusType.Queue), IQueueProvider
{
}