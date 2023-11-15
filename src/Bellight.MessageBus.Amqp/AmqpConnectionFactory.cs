using Amqp;
using Microsoft.Extensions.Options;

namespace Bellight.MessageBus.Amqp;

public class AmqpConnectionFactory(IOptionsMonitor<AmqpOptions> options) : IAmqpConnectionFactory
{
    private readonly IOptionsMonitor<AmqpOptions> options = options;
    private Connection? connection;
    private readonly Dictionary<string, Session> sessions = new();

    public Connection GetConnection()
    {
        if (connection?.IsClosed != false)
        {
            connection = new Connection(new Address(options.CurrentValue.Endpoint));
        }

        return connection;
    }

    public Session GetSession(string name = "default")
    {
        if (sessions.TryGetValue(name, out Session? value))
        {
            var session = value;
            if (session is not null && !session.IsClosed)
            {
                return session;
            }
        }
        var connection = GetConnection();
        var newSession = new Session(connection);
        sessions.Add(name, newSession);

        return newSession;
    }
}