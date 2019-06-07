using Bellight.Configurations;

namespace Bellight.Queue.Abstractions
{
    public class QueueOptions : IAppSettingSection
    {
        public string WorkerQueueName { get; set; }
        public string SchedulerQueueName { get; set; }
        public int QueuePollingInterval { get; set; } = Constants.QueuePollingIntervalDefaultMs;
    }
}
