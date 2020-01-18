using Bellight.MessageBus.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Bellight.MessageBus.Amqp
{
    public class AmqpPubsubProvider : AmqpProviderBase, IPubsubProvider
    {
        public AmqpPubsubProvider(IOptionsMonitor<AmqpOptions> options)
            : base(options, MessageBusType.PubSub)
        {
        }
    }
}
