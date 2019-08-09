using Amqp;
using Amqp.Framing;
using Amqp.Types;
using Bellight.Core.Misc;
using Bellight.MessageBus.Abstractions;
using Microsoft.Extensions.Options;
using System;
using System.Threading;

namespace Bellight.MessageBus.Amqp
{
    public class AmqpReceiverLinkWrapper : AmqpLinkWrapper<ReceiverLink>
    {
        private const string _linkName = "receiver-link";
        private readonly Action<string> _messageReceivedAction;
        private readonly IOptionsMonitor<AmqpOptions> _options;
        private readonly string _topic;
        private readonly MessageBusType _messageBusType;
        private readonly CancellationTokenSource _tokenSource;

        public AmqpReceiverLinkWrapper(
            string topic, 
            IOptionsMonitor<AmqpOptions> options, 
            Action<string> messageReceivedAction, 
            MessageBusType messageBusType) : base(options)
        {
            _messageReceivedAction = messageReceivedAction;
            _options = options;
            _topic = topic;
            _tokenSource = new CancellationTokenSource();

            StartPolling(_tokenSource.Token);
            _messageBusType = messageBusType;
        }

        protected override ReceiverLink InitialiseLink(Session session)
        {
            var source = new Source
            {
                Address = _topic,
                Capabilities = new Symbol[] {
                    new Symbol(_messageBusType == MessageBusType.Queue ? "queue" : "topic")
                }
            };

            return new ReceiverLink(session, _linkName, source, null);
        }

        public void PollMessage(CancellationToken cancellationToken)
        {
            while(!cancellationToken.IsCancellationRequested)
            {
                var options = _options.CurrentValue;
                SafeExecute.SyncCatch(() =>
                {
                    var link = GetLink();
                    var message = link.Receive(TimeSpan.FromMilliseconds(options.PollingIntervalMilliseconds));
                    if (message == null)
                    {
                        Thread.Sleep(options.WaitDurationMilliseconds);
                        return;
                    }

                    link.Accept(message);
                    _messageReceivedAction.Invoke((string)message.Body);
                }, () => Thread.Sleep(options.WaitDurationMilliseconds));
                
            }
        }

        public override void Dispose()
        {
            _tokenSource.Cancel();
            Thread.Sleep(_options.CurrentValue.WaitDurationMilliseconds);
            base.Dispose();
        }

        private void StartPolling(CancellationToken stoppingToken)
        {
            SafeExecute.Sync(() => ThreadPool.QueueUserWorkItem(s => PollMessage((CancellationToken)s), stoppingToken));
        }
    }
}
