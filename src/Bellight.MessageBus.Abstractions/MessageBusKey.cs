namespace Bellight.MessageBus.Abstractions;

internal struct MessageBusKey
{
    public string Topic { get; set; }
    public MessageBusType MessageBusType { get; set; }
}