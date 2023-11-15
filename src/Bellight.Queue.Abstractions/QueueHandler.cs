using System;
using System.Threading;

namespace Bellight.Queue.Abstractions;

public class QueueHandler
{
    public string QueueName { get; set; }
    public bool Wait { get; set; }
    public DateTimeOffset LastActivityTime { get; set; }
    public CancellationTokenSource TokenSource { get; set; } = new CancellationTokenSource();
}
