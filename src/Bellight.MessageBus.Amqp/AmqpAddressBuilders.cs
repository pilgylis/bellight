using Bellight.MessageBus.Abstractions;

namespace Bellight.MessageBus.Amqp;

public class AmqpAddressBuilders
{
    public Func<string, MessageBusType, string> PublisherAddress { get; set; } =
        (topic, type) => type == MessageBusType.Queue
            ? $"/queues/{topic}"
            : $"/exchanges/{topic}";

    public Func<string, MessageBusType, AmqpOptions, string> SubscriberAddress { get; set; } =
        (topic, type, options) =>
        {
            if ("true".Equals(options.IsAzureMessageBus, StringComparison.InvariantCultureIgnoreCase)
                && type == MessageBusType.PubSub)
            {
                return $"{topic}/Subscriptions/{options.SubscriberName}";
            }

            return type == MessageBusType.Queue
                ? $"/queues/{topic}"
                : $"/queues/{topic}.{options.SubscriberName}";
        };
}
