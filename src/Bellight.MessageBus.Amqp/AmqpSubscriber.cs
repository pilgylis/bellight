﻿using Amqp;
using Amqp.Framing;
using Amqp.Types;
using Bellight.Core.Misc;
using Bellight.MessageBus.Abstractions;

namespace Bellight.MessageBus.Amqp;

public class AmqpSubscriber(IAmqpConnectionFactory connectionFactory, SubscriberOptions options) : AmqpLinkWrapper<ReceiverLink>(connectionFactory), ISubscriber
{
    private const string _linkName = "receiver-link";

    private readonly SubscriberOptions _options = options;

    public ISubscription Subscribe(Action<string> messageReceivedAction)
    {
        var tokenSource = new CancellationTokenSource();
        SafeExecute.Sync(() => ThreadPool.QueueUserWorkItem(s =>
        {
            var cancellationToken = (CancellationToken)s!;
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
        if ("true".Equals(_options.IsAzureMessageBus, StringComparison.InvariantCultureIgnoreCase)
            && _options.MessageBusType == MessageBusType.PubSub)
        {
            return new ReceiverLink(session, _linkName, $"{_options.Topic}/Subscriptions/{_options.SubscriberName}");
        }

        var source = new Source
        {
            Address = _options.Topic,
            Capabilities = [
                new Symbol(_options.MessageBusType == MessageBusType.Queue ? "queue" : "topic")
            ]
        };

        return new ReceiverLink(session, _linkName, source, null);
    }

    private void PollMessage(Action<string> messageReceivedAction, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            SafeExecute.AsyncCatch(async () =>
            {
                var link = GetLink();
                var message = await link.ReceiveAsync(TimeSpan.FromMilliseconds(_options.PollingInterval));
                if (message == null)
                {
                    await Task.Delay(_options.WaitDuration);
                    return;
                }

                link.Accept(message);
                messageReceivedAction.Invoke((string)message.Body);
            }, () => Thread.Sleep(_options.WaitDuration)).Wait(cancellationToken);
        }
    }
}