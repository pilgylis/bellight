using System;

namespace Bellight.Queue.Abstractions
{
    public class QueueMessage
    {
        public Type Type { get; set; }
        public string Data { get; set; }
    }
}
