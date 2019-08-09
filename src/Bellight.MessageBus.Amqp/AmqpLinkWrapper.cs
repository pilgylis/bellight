using Amqp;
using Microsoft.Extensions.Options;
using System;

namespace Bellight.MessageBus.Amqp
{
    public abstract class AmqpLinkWrapper<T>: IDisposable where T: ILink
    {

        private Connection _connection;
        private Session _session;
        private T _link;

        protected IOptionsMonitor<AmqpOptions> Options { get; }

        public AmqpLinkWrapper(IOptionsMonitor<AmqpOptions> options) {
            Options = options;
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
            if (_connection?.IsClosed != false)
            {
                _connection = new Connection(new Address(Options.CurrentValue.Endpoint));
            }

            if (_session?.IsClosed != false)
            {
                _session = new Session(_connection);
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

            if (_connection?.IsClosed == false)
            {
                _connection.Close();
            }
        }
    }
}
