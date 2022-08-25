using Bellight.MessageBus.Abstractions;

namespace Bellight.MessageBus.Amqp;

public class SubscriberOptions
{
    public string? Topic { get; set; }
    public string? SubscriberName { get; set; }
    public MessageBusType MessageBusType { get; set; }
    public int PollingInterval { get; set; }
    public int WaitDuration { get; set; }
    public string? IsAzureMessageBus { get; set; }
}