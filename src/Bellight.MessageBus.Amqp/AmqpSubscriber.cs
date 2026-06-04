using Amqp;
using Amqp.Framing;
using Amqp.Types;
using Bellight.MessageBus.Abstractions;
using Microsoft.Extensions.Logging;

namespace Bellight.MessageBus.Amqp;

public class AmqpSubscriber(IAmqpConnectionFactory connectionFactory, ILogger logger, SubscriberOptions options) 
    : AmqpLinkWrapper<ReceiverLink>(connectionFactory), ISubscriber
{
    private const string _linkName = "receiver-link";

    public ISubscription Subscribe(Func<string, Task> messageReceivedAction)
    {
        var tokenSource = new CancellationTokenSource();

        Task.Run(async () =>
        {
            while (!tokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    await PollMessage(messageReceivedAction, tokenSource.Token).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while polling messages: {ErrorMessage}", ex.Message);
                    await Task.Delay(options.WaitDuration, tokenSource.Token).ConfigureAwait(false);
                }
            }
        });
        
        return new DefaultSubscription(tokenSource);
    }

    protected override ReceiverLink InitialiseLink(Session session)
    {
        if ("true".Equals(options.IsAzureMessageBus, StringComparison.InvariantCultureIgnoreCase)
            && options.MessageBusType == MessageBusType.PubSub)
        {
            return new ReceiverLink(session, _linkName, $"{options.Topic}/Subscriptions/{options.SubscriberName}");
        }

        var address = options.MessageBusType == MessageBusType.Queue
            ? $"/queues/{options.Topic}"
            : $"/queues/{options.Topic}.{options.SubscriberName}";

        return new ReceiverLink(session, _linkName, address);
    }


    private async Task PollMessage(Func<string, Task> messageReceivedAction, CancellationToken cancellationToken)
    {
        logger.LogDebug("Polling for messages on topic '{Topic}'...", options.Topic);
        var link = GetLink();
        var message = await link.ReceiveAsync(TimeSpan.FromMilliseconds(options.PollingInterval));
        if (message == null)
        {
            await Task.Delay(options.WaitDuration, cancellationToken);
            return;
        }

        link.Accept(message);
        await messageReceivedAction.Invoke((string)message.Body).ConfigureAwait(false);
    }
}