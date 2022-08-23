namespace Bellight.MessageBus.Amqp;

internal static class Constants
{
    public const string EndpointConfig = "Amqp:Endpoint";
    public const int DefaultReceiverQueueIntervalInSeconds = 1;
    public const int DefaultPollingIntervalMilliseconds = 2000;
    public const int DefaultWaitDurationMilliseconds = 2000;
}
