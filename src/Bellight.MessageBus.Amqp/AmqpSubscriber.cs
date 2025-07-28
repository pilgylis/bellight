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

    public ISubscription Subscribe(Func<string, Task> messageReceivedAction)
    {
        var tokenSource = new CancellationTokenSource();
        SafeExecute.Sync(() => ThreadPool.QueueUserWorkItem(s =>
        {
            var cancellationToken = (CancellationToken)s!;
            while (!cancellationToken.IsCancellationRequested)
            {
                PollMessage(messageReceivedAction, cancellationToken)
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult();
            }
        }, tokenSource.Token));
        return new DefaultSubscription(tokenSource);
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

    private async Task PollMessage(Func<string, Task> messageReceivedAction, CancellationToken cancellationToken)
    {
        await SafeExecute.AsyncCatch(async () =>
        {
            var link = GetLink();
            var message = await link.ReceiveAsync(TimeSpan.FromMilliseconds(options.PollingInterval));
            if (message == null)
            {
                await Task.Delay(options.WaitDuration, cancellationToken);
                return;
            }

            link.Accept(message);
            await messageReceivedAction.Invoke((string)message.Body).ConfigureAwait(false);
        }, () => Task.Delay(options.WaitDuration, cancellationToken).Wait()).ConfigureAwait(false);
    }
}