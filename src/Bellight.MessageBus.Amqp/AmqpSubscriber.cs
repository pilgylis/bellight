using Amqp;
using Amqp.Framing;
using Amqp.Types;
using Bellight.Core.Misc;
using Bellight.MessageBus.Abstractions;
using System;
using System.Threading;

namespace Bellight.MessageBus.Amqp
{
    public class AmqpSubscriber : AmqpLinkWrapper<ReceiverLink>, ISubscriber
    {
        private const string _linkName = "receiver-link";
        private readonly SubscriberOptions _options;

        public AmqpSubscriber(SubscriberOptions options) : base(options.Endpoint)
        {
            _options = options;
        }

        public ISubscription Subscribe(Action<string> messageReceivedAction)
        {
            var tokenSource = new CancellationTokenSource();
            SafeExecute.Sync(() => ThreadPool.QueueUserWorkItem(s => {
                var cancellationToken = (CancellationToken)s;
                while (!cancellationToken.IsCancellationRequested)
                {
                    SafeExecute.SyncCatch(
                        () => PollMessage(messageReceivedAction, cancellationToken),
                        () => Thread.Sleep(_options.WaitDuration));
                }
            }, tokenSource.Token));
            return new DefaultSubscription(() => tokenSource.Cancel());
        }

        protected override ReceiverLink InitialiseLink(Session session)
        {
            var source = new Source
            {
                Address = _options.Topic,
                Capabilities = new Symbol[] {
                    new Symbol(_options.MessageBusType == MessageBusType.Queue ? "queue" : "topic")
                }
            };

            return new ReceiverLink(session, _linkName, source, null);
        }

        private void PollMessage(Action<string> messageReceivedAction, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                SafeExecute.SyncCatch(() =>
                {
                    var link = GetLink();
                    var message = link.Receive(TimeSpan.FromMilliseconds(_options.PollingInterval));
                    if (message == null)
                    {
                        Thread.Sleep(_options.WaitDuration);
                        return;
                    }

                    link.Accept(message);
                    messageReceivedAction.Invoke((string)message.Body);
                }, () => Thread.Sleep(_options.WaitDuration));

            }
        }
    }
}
