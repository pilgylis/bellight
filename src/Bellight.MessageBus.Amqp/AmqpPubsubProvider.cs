using Bellight.MessageBus.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bellight.MessageBus.Amqp;

public class AmqpPubsubProvider(
    IAmqpConnectionFactory connectionFactory,
    ILogger<AmqpPubsubProvider> logger,
    IOptionsMonitor<AmqpOptions> options,
    AmqpAddressBuilders addressBuilders) : AmqpProviderBase(
        connectionFactory,
        logger,
        options,
        addressBuilders,
        MessageBusType.PubSub), IPubsubProvider
{
}