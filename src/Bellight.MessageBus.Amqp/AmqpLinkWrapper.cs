using Amqp;

namespace Bellight.MessageBus.Amqp;

public abstract class AmqpLinkWrapper<T>(
    IAmqpConnectionFactory connectionFactory) : IDisposable where T : class, ILink
{
    private T? _link;

    protected abstract T InitialiseLink(Session session);

    public T GetLink()
    {
        if (_link != null && !_link.IsClosed)
        {
            return _link;
        }

        var session = connectionFactory.GetSession();
        _link = InitialiseLink(session);
        return _link;
    }

    /// <summary>
    /// Discards the current link and tears down the shared connection/session, so the
    /// next <see cref="GetLink"/> call is forced to establish a fresh one. Call this when
    /// a link operation fails without the link itself becoming <c>IsClosed</c> (e.g. a send
    /// timeout against a transport that died silently) - otherwise <see cref="GetLink"/>
    /// keeps handing back the same wedged link forever.
    /// </summary>
    protected void Invalidate()
    {
        var link = _link;
        _link = null;

        if (link?.IsClosed == false)
        {
            try
            {
                link.Close();
            }
            catch
            {
                // best-effort: the link is already being discarded either way
            }
        }

        connectionFactory.Reset();
    }

    public void Dispose()
    {
        if (_link?.IsClosed == false)
        {
            _link.Close();
        }
        GC.SuppressFinalize(this);
    }
}