using Amqp;

namespace Bellight.MessageBus.Amqp
{
    public abstract class AmqpLinkWrapper<T> : IDisposable where T : ILink
    {
        private Session? _session;
        private T? _link;
        private readonly IAmqpConnectionFactory connectionFactory;

        public AmqpLinkWrapper(IAmqpConnectionFactory connectionFactory)
        {
            this.connectionFactory = connectionFactory;
        }

        protected abstract T InitialiseLink(Session session);

        public T GetLink()
        {
            if (_link != null && !_link.IsClosed)
            {
                return _link;
            }

            var session = GetSession();
            _link = InitialiseLink(session);
            return _link;
        }

        private Session GetSession()
        {
            if (_session?.IsClosed != false)
            {
                var connection = connectionFactory.GetConnection();
                _session = new Session(connection);
            }

            return _session;
        }

        public virtual void Dispose()
        {
            if (_link?.IsClosed == false)
            {
                _link.Close();
            }

            if (_session?.IsClosed == false)
            {
                _session.Close();
            }
        }
    }
}
