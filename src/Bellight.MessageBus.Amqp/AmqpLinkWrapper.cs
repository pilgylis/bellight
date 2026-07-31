using Amqp;

namespace Bellight.MessageBus.Amqp;

public abstract class AmqpLinkWrapper<T>(
    IAmqpConnectionFactory connectionFactory,
    string sessionKey) : IDisposable where T : class, ILink
{
    // GetLink()/Invalidate() run concurrently: the IPublisher/ISubscriber returned by
    // IMessageBusFactory.GetPublisher/Subscribe is cached and shared by every caller for a
    // given topic, so two callers can race to (re)create the link at the same time. Without
    // this lock, two concurrent GetLink() calls can both attach a new link with the same
    // name to the broker ("has been attached by a link with the same role"), and a
    // concurrent Invalidate() can close a link out from under an in-flight Send/Receive.
    private readonly Lock _gate = new();
    private T? _link;

    protected abstract T InitialiseLink(Session session);

    public T GetLink()
    {
        lock (_gate)
        {
            if (_link != null && !_link.IsClosed)
            {
                return _link;
            }

            var session = connectionFactory.GetSession(sessionKey);
            _link = InitialiseLink(session);
            return _link;
        }
    }

    /// <summary>
    /// Discards <paramref name="failedLink"/> and tears down this wrapper's own session
    /// (identified by <c>sessionKey</c>) - leaving the shared connection and every other
    /// topic's session untouched - so the next <see cref="GetLink"/> call is forced to
    /// establish a fresh one. Call this when an operation on <paramref name="failedLink"/>
    /// fails without the link itself becoming <c>IsClosed</c> (e.g. a send timeout against a
    /// transport that died silently) - otherwise <see cref="GetLink"/> keeps handing back the
    /// same wedged link forever. Pass the exact link instance the failed operation used: if
    /// another caller has already replaced it with a working link by the time this runs, this
    /// is a no-op instead of discarding their replacement.
    /// </summary>
    protected void Invalidate(T failedLink)
    {
        lock (_gate)
        {
            if (!ReferenceEquals(_link, failedLink))
            {
                return;
            }

            _link = null;

            if (!failedLink.IsClosed)
            {
                try
                {
                    failedLink.Close();
                }
                catch
                {
                    // best-effort: the link is already being discarded either way
                }
            }
        }

        connectionFactory.Reset(sessionKey);
    }

    public void Dispose()
    {
        lock (_gate)
        {
            if (_link?.IsClosed == false)
            {
                _link.Close();
            }
        }
        GC.SuppressFinalize(this);
    }
}