using Bellight.MessageBus.Abstractions;
using Microsoft.Extensions.Options;

namespace Bellight.MessageBus.Amqp;

public class AmqpPubsubProvider(
    IAmqpConnectionFactory connectionFactory, 
    IOptionsMonitor<AmqpOptions> options) : AmqpProviderBase(
        connectionFactory, 
        options, 
        MessageBusType.PubSub), IPubsubProvider
{
}