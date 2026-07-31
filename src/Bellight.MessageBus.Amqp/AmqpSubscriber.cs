using Amqp;
using Bellight.MessageBus.Abstractions;
using Microsoft.Extensions.Logging;

namespace Bellight.MessageBus.Amqp;

public class AmqpSubscriber(IAmqpConnectionFactory connectionFactory, ILogger logger, SubscriberOptions options)
    : AmqpLinkWrapper<ReceiverLink>(connectionFactory, $"{options.MessageBusType}:{options.Topic}"), ISubscriber
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
        return new ReceiverLink(session, _linkName, options.Address);
    }


    private async Task PollMessage(Func<string, Task> messageReceivedAction, CancellationToken cancellationToken)
    {
        logger.LogDebug("Polling for messages on topic '{Topic}'...", options.Topic);
        var link = GetLink();

        // Only the link-touching calls (ReceiveAsync/Accept) indicate a broken link - a
        // cancelled delay or a throwing message handler must not tear down a healthy link.
        Message? message;
        try
        {
            message = await link.ReceiveAsync(TimeSpan.FromMilliseconds(options.PollingInterval));
        }
        catch
        {
            Invalidate(link);
            throw;
        }

        if (message == null)
        {
            await Task.Delay(options.WaitDuration, cancellationToken);
            return;
        }

        try
        {
            link.Accept(message);
        }
        catch
        {
            Invalidate(link);
            throw;
        }

        await messageReceivedAction.Invoke((string)message.Body).ConfigureAwait(false);
    }
}