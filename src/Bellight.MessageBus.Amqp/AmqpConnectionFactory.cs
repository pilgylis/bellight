using Amqp;
using Microsoft.Extensions.Options;

namespace Bellight.MessageBus.Amqp
{
    public class AmqpConnectionFactory : IAmqpConnectionFactory
    {
        private readonly IOptionsMonitor<AmqpOptions> options;
        private Connection? connection;

        public AmqpConnectionFactory(IOptionsMonitor<AmqpOptions> options)
        {
            this.options = options;
        }

        public Connection GetConnection()
        {
            if (connection == null || connection.IsClosed)
            {
                connection = new Connection(new Address(options.CurrentValue.Endpoint));
            }

            return connection;
        }
    }
}
