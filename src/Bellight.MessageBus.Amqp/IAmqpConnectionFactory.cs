using Amqp;

namespace Bellight.MessageBus.Amqp;

public interface IAmqpConnectionFactory
{
    /// <summary>
    /// Gets (creating if needed) the session identified by <paramref name="name"/>, on its
    /// own dedicated connection. Each distinct caller should use its own stable name - e.g.
    /// a publisher/subscriber keys this by its topic - rather than sharing a connection or
    /// session across unrelated topics, so a problem on one topic can't affect another's.
    /// </summary>
    Session GetSession(string name);

    /// <summary>
    /// Force-closes the connection and session identified by <paramref name="name"/> -
    /// leaving every other name's connection/session untouched - so the next
    /// <see cref="GetSession"/> call for that name establishes a fresh one. Call after a
    /// link operation fails in a way that doesn't itself mark the session/link as closed
    /// (e.g. a send timeout against a transport that died silently) - otherwise every
    /// subsequent call for that name keeps reusing the same wedged connection.
    /// </summary>
    void Reset(string name);
}