using Amqp;
using Microsoft.Extensions.Options;

namespace Bellight.MessageBus.Amqp;

public class AmqpConnectionFactory(IOptionsMonitor<AmqpOptions> options) : IAmqpConnectionFactory
{
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
        if (sessions.TryGetValue(name, out var value))
        {
            var session = value;
            if (!session.IsClosed)
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