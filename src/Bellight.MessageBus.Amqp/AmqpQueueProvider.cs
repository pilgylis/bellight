using Bellight.MessageBus.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bellight.MessageBus.Amqp;

public class AmqpQueueProvider(
    IAmqpConnectionFactory connectionFactory,
    ILogger<AmqpQueueProvider> logger,
    IOptionsMonitor<AmqpOptions> options,
    AmqpAddressBuilders addressBuilders) : AmqpProviderBase(
        connectionFactory,
        logger,
        options,
        addressBuilders,
        MessageBusType.Queue), IQueueProvider
{
}