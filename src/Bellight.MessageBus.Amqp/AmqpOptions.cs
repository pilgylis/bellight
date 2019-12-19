namespace Bellight.MessageBus.Amqp
{
    public class AmqpOptions
    {
        public string Endpoint { get; set; }
        public string InstanceName { get; set; }
        public int PollingIntervalMilliseconds { get; set; } = Constants.DefaultPollingIntervalMilliseconds;
        public int WaitDurationMilliseconds { get; set; } = Constants.DefaultWaitDurationMilliseconds;
        public int ReceiverQueueIntervalInSeconds { get; set; } = Constants.DefaultReceiverQueueIntervalInSeconds;
    }
}
