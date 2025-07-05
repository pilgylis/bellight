using Amqp;
using Amqp.Framing;
using Amqp.Types;
using Bellight.Core.Misc;
using Bellight.MessageBus.Abstractions;

namespace Bellight.MessageBus.Amqp;

public class AmqpSubscriber(IAmqpConnectionFactory connectionFactory, SubscriberOptions options) 
    : AmqpLinkWrapper<ReceiverLink>(connectionFactory), ISubscriber
{
    private const string _linkName = "receiver-link";

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
                    () => Thread.Sleep(options.WaitDuration));
            }
        }, tokenSource.Token));
        return new DefaultSubscription(() => tokenSource.Cancel());
    }

    protected override ReceiverLink InitialiseLink(Session session)
    {
        if ("true".Equals(options.IsAzureMessageBus, StringComparison.InvariantCultureIgnoreCase)
            && options.MessageBusType == MessageBusType.PubSub)
        {
            return new ReceiverLink(session, _linkName, $"{options.Topic}/Subscriptions/{options.SubscriberName}");
        }

        var source = new Source
        {
            Address = options.Topic,
            Capabilities = [
                new Symbol(options.MessageBusType == MessageBusType.Queue ? "queue" : "topic")
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
                var message = await link.ReceiveAsync(TimeSpan.FromMilliseconds(options.PollingInterval));
                if (message == null)
                {
                    await Task.Delay(options.WaitDuration, cancellationToken);
                    return;
                }

                link.Accept(message);
                messageReceivedAction.Invoke((string)message.Body);
            }, () => Thread.Sleep(options.WaitDuration)).Wait(cancellationToken);
        }
    }
}