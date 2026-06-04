using Bellight.MessageBus.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

internal sealed class PublisherWorker(
    IMessageBusFactory messageBusFactory,
    ILogger<PublisherWorker> logger,
    IHostApplicationLifetime lifetime) : IHostedService
{
    private static readonly string[] Messages =
    [
        "The quick brown fox jumps over the lazy dog.",
        "Pack my box with five dozen liquor jugs.",
        "How vexingly quick daft zebras jump!",
        "The five boxing wizards jump quickly.",
        "Sphinx of black quartz, judge my vow.",
        "Two driven jocks help fax my big quiz.",
        "Five quacking zephyrs jolt my wax bed.",
        "The jay, pig, fox, zebra, and my wolves quack!",
        "Blowzy red-faced women that thump and jolt.",
        "Joaquin Phoenix was gazed by MTV for luck."
    ];

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Queue example: messages are consumed by a single receiver
        await SendBatchAsync("queue-demo", MessageBusType.Queue, cancellationToken);

        // Pub/Sub example: messages are broadcast to all subscribers
        await SendBatchAsync("pubsub-demo", MessageBusType.PubSub, cancellationToken);

        lifetime.StopApplication();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task SendBatchAsync(string topic, MessageBusType busType, CancellationToken cancellationToken)
    {
        var typeText = busType == MessageBusType.Queue ? "queue" : "topic";
        logger.LogInformation("Sending messages to {Type} '{Topic}'", typeText, topic);

        var publisher = messageBusFactory.GetPublisher(topic, busType);
        for (var i = 0; i < Messages.Length && !cancellationToken.IsCancellationRequested; i++)
        {
            await publisher.SendAsync(Messages[i]);
            logger.LogInformation("{Count} message(s) sent to '{Topic}'", i + 1, topic);
        }
    }
}
