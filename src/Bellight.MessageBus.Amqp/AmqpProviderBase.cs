using Bellight.MessageBus.Abstractions;
using Microsoft.Extensions.Options;

namespace Bellight.MessageBus.Amqp;

public abstract class AmqpProviderBase(
    IAmqpConnectionFactory connectionFactory,
    IOptionsMonitor<AmqpOptions> options,
    MessageBusType messageBusType)
{
    public IPublisher GetPublisher(string topic)
    {
        return new AmqpPublisher(connectionFactory, NormalizeTopic(topic), messageBusType);
    }

    public ISubscription Subscribe(string topic, Action<string> messageReceivedAction)
    {
        var optionsValue = options.CurrentValue;
        var subscriberOptions = new SubscriberOptions
        {
            Topic = NormalizeTopic(topic),
            MessageBusType = messageBusType,
            PollingInterval = optionsValue.PollingIntervalMilliseconds,
            WaitDuration = optionsValue.WaitDurationMilliseconds,
            IsAzureMessageBus = optionsValue.IsAzureMessageBus,
            SubscriberName = optionsValue.SubscriberName
        };

        var subscriber = new AmqpSubscriber(connectionFactory, subscriberOptions);

        return subscriber.Subscribe(messageReceivedAction);
    }

    private string NormalizeTopic(string topic) => string.IsNullOrEmpty(options.CurrentValue.InstanceName) ?
        topic : $"{options.CurrentValue.InstanceName}.{topic}";
}