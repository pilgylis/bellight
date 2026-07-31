using System.Collections.Concurrent;
using Amqp;
using Microsoft.Extensions.Options;

namespace Bellight.MessageBus.Amqp;

public class AmqpConnectionFactory(IOptionsMonitor<AmqpOptions> options) : IAmqpConnectionFactory
{
    private Connection? connection;
    private readonly ConcurrentDictionary<string, Session> sessions = [];

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

        GetConnection();

        var newSession = new Session(connection);
        sessions[name] = newSession;

        return newSession;
    }

    public void Reset()
    {
        var current = connection;
        connection = null;

        foreach (var name in sessions.Keys.ToList())
        {
            if (sessions.TryRemove(name, out var session) && !session.IsClosed)
            {
                TryClose(session);
            }
        }

        if (current?.IsClosed == false)
        {
            TryClose(current);
        }
    }

    private static void TryClose(AmqpObject amqpObject)
    {
        try
        {
            amqpObject.Close();
        }
        catch
        {
            // best-effort: the object is already being discarded either way
        }
    }
}