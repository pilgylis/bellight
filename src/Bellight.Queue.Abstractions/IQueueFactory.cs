namespace Bellight.Queue.Abstractions;

public interface IQueueFactory : ITransientDependency
{
    IQueueService Create();
}
