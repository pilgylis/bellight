using Bellight.MessageBus.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bellight.MessageBus.Amqp;

public abstract class AmqpProviderBase(
    IAmqpConnectionFactory connectionFactory,
    ILogger logger,
    IOptionsMonitor<AmqpOptions> options,
    AmqpAddressBuilders addressBuilders,
    MessageBusType messageBusType)
{
    public IPublisher GetPublisher(string topic)
    {
        var normalizedTopic = NormalizeTopic(topic);
        var publisherOptions = new PublisherOptions
        {
            Topic = normalizedTopic,
            MessageBusType = messageBusType,
            Address = addressBuilders.PublisherAddress(normalizedTopic, messageBusType)
        };
        return new AmqpPublisher(connectionFactory, publisherOptions);
    }

    public ISubscription Subscribe(string topic, Func<string, Task> messageReceivedAction)
    {
        var optionsValue = options.CurrentValue;
        var normalizedTopic = NormalizeTopic(topic);
        var subscriberOptions = new SubscriberOptions
        {
            Topic = normalizedTopic,
            MessageBusType = messageBusType,
            PollingInterval = optionsValue.PollingIntervalMilliseconds,
            WaitDuration = optionsValue.WaitDurationMilliseconds,
            IsAzureMessageBus = optionsValue.IsAzureMessageBus,
            SubscriberName = optionsValue.SubscriberName,
            Address = addressBuilders.SubscriberAddress(normalizedTopic, messageBusType, optionsValue)
        };

        var subscriber = new AmqpSubscriber(connectionFactory, logger, subscriberOptions);

        return subscriber.Subscribe(messageReceivedAction);
    }

    private string NormalizeTopic(string topic) => string.IsNullOrEmpty(options.CurrentValue.InstanceName) ?
        $"{topic}" : $"{options.CurrentValue.InstanceName}.{topic}";
}