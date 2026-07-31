using Amqp;
using Microsoft.Extensions.Options;

namespace Bellight.MessageBus.Amqp;

public class AmqpConnectionFactory(IOptionsMonitor<AmqpOptions> options) : IAmqpConnectionFactory
{
    // Every named caller gets its own dedicated connection (not just its own session on a
    // shared connection): establishing a second session on an already-active connection
    // was observed to hang in practice, so isolation is at the connection level - the one
    // thing proven to actually contain a topic's problems to itself.
    private readonly Lock _gate = new();
    private readonly Dictionary<string, Connection> connections = [];
    private readonly Dictionary<string, Session> sessions = [];

    public Session GetSession(string name)
    {
        lock (_gate)
        {
            if (sessions.TryGetValue(name, out var value) && !value.IsClosed)
            {
                return value;
            }

            var newSession = new Session(GetConnectionNoLock(name));
            sessions[name] = newSession;
            return newSession;
        }
    }

    public void Reset(string name)
    {
        lock (_gate)
        {
            if (sessions.Remove(name, out var session) && !session.IsClosed)
            {
                TryClose(session);
            }

            if (connections.Remove(name, out var conn) && !conn.IsClosed)
            {
                TryClose(conn);
            }
        }
    }

    private Connection GetConnectionNoLock(string name)
    {
        if (connections.TryGetValue(name, out var existing) && !existing.IsClosed)
        {
            return existing;
        }

        var newConnection = new Connection(new Address(options.CurrentValue.Endpoint));
        connections[name] = newConnection;
        return newConnection;
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