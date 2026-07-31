using Amqp;

namespace Bellight.MessageBus.Amqp;

public interface IAmqpConnectionFactory
{
    Connection GetConnection();

    Session GetSession(string name = "default");

    /// <summary>
    /// Force-closes the current connection and all cached sessions so the next
    /// <see cref="GetConnection"/>/<see cref="GetSession"/> call establishes fresh ones.
    /// Call after a link operation fails in a way that doesn't itself mark the
    /// connection/session/link as closed (e.g. a send timeout against a silently
    /// dead transport) - otherwise every subsequent call keeps reusing the same
    /// wedged connection.
    /// </summary>
    void Reset();
}