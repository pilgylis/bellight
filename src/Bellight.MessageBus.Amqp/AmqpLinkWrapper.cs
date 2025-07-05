using Amqp;

namespace Bellight.MessageBus.Amqp;

public abstract class AmqpLinkWrapper<T>(IAmqpConnectionFactory connectionFactory) : IDisposable where T : ILink
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

    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
        if (_link?.IsClosed == false)
        {
            _link.Close();
        }
    }
}