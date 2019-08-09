namespace Bellight.MessageBus.Amqp
{
    public static class Constants
    {
        public static readonly string EndpointConfig = "Amqp:Endpoint";
        public const int DefaultReceiverQueueIntervalInSeconds = 1;
        public const int DefaultPollingIntervalMilliseconds = 2000;
        public const int DefaultWaitDurationMilliseconds = 2000;
    }
}
