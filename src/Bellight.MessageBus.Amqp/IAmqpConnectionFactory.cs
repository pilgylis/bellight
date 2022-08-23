using Amqp;

namespace Bellight.MessageBus.Amqp;

public interface IAmqpConnectionFactory
{
    Connection GetConnection();
    Session GetSession(string name = "default");
}
