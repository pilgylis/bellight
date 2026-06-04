using Bellight.MessageBus.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

internal sealed class SubscriberWorker(
    IMessageBusFactory messageBusFactory,
    ILogger<SubscriberWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Queue example: only one subscriber receives each message
        logger.LogInformation("Subscribing queue 'queue-demo'");
        using var queueSubscription = messageBusFactory.Subscribe(
            "queue-demo", OnQueueMessageReceived, MessageBusType.Queue);

        // Pub/Sub example: every subscriber receives every message
        logger.LogInformation("Subscribing topic 'pubsub-demo'");
        using var pubsubSubscription = messageBusFactory.Subscribe(
            "pubsub-demo", OnPubSubMessageReceived, MessageBusType.PubSub);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private Task OnQueueMessageReceived(string message)
    {
        logger.LogInformation("[Queue] Received: {Message}", message);
        return Task.CompletedTask;
    }

    private Task OnPubSubMessageReceived(string message)
    {
        logger.LogInformation("[PubSub] Received: {Message}", message);
        return Task.CompletedTask;
    }
}
