using Bellight.MessageBus.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Bellight.MessageBus.Amqp
{
    public class AmqpPubsubProvider : AmqpProviderBase, IPubsubProvider
    {
        public AmqpPubsubProvider(IConfiguration configuration, IOptionsMonitor<AmqpOptions> options)
            : base(configuration, options, MessageBusType.PubSub)
        {
        }
    }
}
