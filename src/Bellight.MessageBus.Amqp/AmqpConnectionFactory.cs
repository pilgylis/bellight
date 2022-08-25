using Amqp;
using Microsoft.Extensions.Options;

namespace Bellight.MessageBus.Amqp;

public class AmqpConnectionFactory : IAmqpConnectionFactory
{
    private readonly IOptionsMonitor<AmqpOptions> options;
    private Connection? connection;
    private readonly Dictionary<string, Session> sessions = new();

    public AmqpConnectionFactory(IOptionsMonitor<AmqpOptions> options)
    {
        this.options = options;
    }

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
        if (sessions.ContainsKey(name))
        {
            var session = sessions[name];
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