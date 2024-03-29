﻿using Bellight.MessageBus.Abstractions;
using Microsoft.Extensions.Options;

namespace Bellight.MessageBus.Amqp;

public abstract class AmqpProviderBase(
    IAmqpConnectionFactory connectionFactory,
    IOptionsMonitor<AmqpOptions> options,
    MessageBusType messageBusType)
{
    private readonly MessageBusType _messageBusType = messageBusType;
    private readonly IAmqpConnectionFactory connectionFactory = connectionFactory;
    private readonly IOptionsMonitor<AmqpOptions> _options = options;

    public IPublisher GetPublisher(string topic)
    {
        return new AmqpPublisher(connectionFactory, NormalizeTopic(topic), _messageBusType);
    }

    public ISubscription Subscribe(string topic, Action<string> messageReceivedAction)
    {
        var options = _options.CurrentValue;
        var subscriberOptions = new SubscriberOptions
        {
            Topic = NormalizeTopic(topic),
            MessageBusType = _messageBusType,
            PollingInterval = options.PollingIntervalMilliseconds,
            WaitDuration = options.WaitDurationMilliseconds,
            IsAzureMessageBus = options.IsAzureMessageBus,
            SubscriberName = options.SubscriberName
        };

        var subscriber = new AmqpSubscriber(connectionFactory, subscriberOptions);

        return subscriber.Subscribe(messageReceivedAction);
    }

    private string NormalizeTopic(string topic) => string.IsNullOrEmpty(_options.CurrentValue.InstanceName) ?
        topic : $"{_options.CurrentValue.InstanceName}.{topic}";
}