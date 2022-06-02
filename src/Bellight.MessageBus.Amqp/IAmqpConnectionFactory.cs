using Amqp;

namespace Bellight.MessageBus.Amqp
{
    public interface IAmqpConnectionFactory
    {
        Connection GetConnection();
    }
}
