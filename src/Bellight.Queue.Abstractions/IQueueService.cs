using Bellight.Core;
using System;
using System.Threading.Tasks;

namespace Bellight.Queue.Abstractions
{
    public interface IQueueService : ISingletonDependency
    {
        Task EnqueueAsync<T>(string topic, T message) where T : class;
        IObservable<T> GetObservableTopic<T>(string topic, QueueHandler handler = null) where T : class;
    }
}
