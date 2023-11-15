using Bellight.Core.Misc;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Microsoft.Extensions.Options;

namespace Bellight.Queue.Abstractions;

public abstract class QueueServiceBase<CT> : IQueueService, IDisposable where CT : class
{
    private readonly ISerializer _serializer;
    private readonly IOptions<QueueOptions> _options;

    public QueueServiceBase(ISerializer serializer, IOptions<QueueOptions> options)
    {
        _serializer = serializer;
        _options = options;
    }

    public Task EnqueueAsync<T>(string topic, T message) where T : class
    {
        var msg = new QueueMessage
        {
            Type = message.GetType(),
            Data = _serializer.SerializeObject(message)
        };

        var msgText = _serializer.SerializeObject(msg);

        return EnqueueAsync(topic, msgText);
    }

    public IObservable<T> GetObservableTopic<T>(string topic, QueueHandler handler = null) where T : class
    {
        return Observable.Create<T>(observer => {

            var tokenSource = handler != null ? handler.TokenSource : new CancellationTokenSource();
            var stoppingToken = tokenSource.Token;

            ThreadPool.QueueUserWorkItem(s => {
                var token = (CancellationToken)s;

                var consumer = CreateConsumer(topic);

                while (!token.IsCancellationRequested)
                {
                    while (handler != null && handler.Wait)
                    {
                        handler.LastActivityTime = DateTimeOffset.Now;
                        Wait();
                    }

                    var pollingInterval = 500;
                    SafeExecute.Sync(() => pollingInterval = _options.Value.QueuePollingInterval);
                    if (pollingInterval < 500)
                    {
                        pollingInterval = 500;
                    }

                    var messageText = string.Empty;

                    try
                    {
                        messageText = PollMessage(consumer, pollingInterval).RunSync();

                        if (handler != null)
                        {
                            handler.LastActivityTime = DateTimeOffset.Now;
                        }

                        if (string.IsNullOrEmpty(messageText))
                        {
                            Wait();
                            continue;
                        }

                    }
                    catch (OperationCanceledException)
                    {
                        // do nothing
                    }
                    catch (Exception e)
                    {
                        StaticLog.Error(e, "Queue consume error: {0}");
                    }

                    try
                    {
                        var messageWrapper = _serializer.DeserializeObject<QueueMessage>(messageText);
                        var item = _serializer.DeserializeObject(messageWrapper.Data, messageWrapper.Type) as T;
                        observer.OnNext(item);
                    }
                    catch (Exception e)
                    {
                        StaticLog.Error($"Deserialise message error: {messageText} - {e.Message}");
                    }
                }
            }, stoppingToken);

            return () => {
                tokenSource.Cancel();
            };
        });
    }

    private void Wait()
    {
        var pollingInterval = _options.Value.QueuePollingInterval;
        if (pollingInterval < 500)
        {
            pollingInterval = 500;
        }

        Thread.Sleep(pollingInterval); // So that we don't stress the server
    }

    protected abstract CT CreateConsumer(string topic);
    protected abstract Task<string> PollMessage(CT consumer, int pollingInterval);

    protected abstract Task EnqueueAsync(string topic, string message);
    public virtual void Dispose() { }
}
