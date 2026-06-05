using Bellight.MessageBus.Abstractions;

namespace Bellight.MessageBus.Amqp;

public class PublisherOptions
{
    public string? Topic { get; set; }
    public MessageBusType MessageBusType { get; set; }
    public string? Address { get; set; }
}
