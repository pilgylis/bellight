namespace Bellight.MessageBus.Abstractions;
public class MessageBusProviderNotFoundException : Exception
{
    public MessageBusProviderNotFoundException()
    {
    }

    public MessageBusProviderNotFoundException(string message) : base(message)
    {
    }

    public MessageBusProviderNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}