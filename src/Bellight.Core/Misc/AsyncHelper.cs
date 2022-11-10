﻿namespace Bellight.Core.Misc;

public static class AsyncHelpers
{
    /// <summary>
    /// Execute's an async Task<T> method which has a void return value synchronously
    /// </summary>
    /// <param name="task">Task<T> method to execute</param>
    public static void RunSyncVoid(this Func<Task> taskFactory)
    {
        RunSyncVoid(taskFactory());
    }

    public static void RunSyncVoid(this Task task)
    {
        var oldContext = SynchronizationContext.Current;
        var synch = new ExclusiveSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(synch);
        synch.Post(async _ =>
        {
            try
            {
                await task;
            }
            catch (Exception e)
            {
                synch.InnerException = e;
                throw;
            }
            finally
            {
                synch.EndMessageLoop();
            }
        }, null!);
        synch.BeginMessageLoop();

        SynchronizationContext.SetSynchronizationContext(oldContext);
    }

    public static void RunSyncVoidSafe(this Func<Task> task)
    {
        SafeExecute.Sync(() => RunSyncVoid(task));
    }

    public static void RunSyncVoidSafe(this Task task)
    {
        SafeExecute.Sync(() => RunSyncVoid(task));
    }

    /// <summary>
    /// Execute's an async Task<T> method which has a T return type synchronously
    /// </summary>
    /// <typeparam name="T">Return Type</typeparam>
    /// <param name="task">Task<T> method to execute</param>
    /// <returns></returns>
    public static T RunSync<T>(this Func<Task<T>> taskFactory)
    {
        return RunSync(taskFactory());
    }

    public static T RunSync<T>(this Task<T> task)
    {
        var oldContext = SynchronizationContext.Current;
        var synch = new ExclusiveSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(synch);
        T ret = default!;
        synch.Post(async _ =>
        {
            try
            {
                ret = await task;
            }
            catch (Exception e)
            {
                synch.InnerException = e;
                throw;
            }
            finally
            {
                synch.EndMessageLoop();
            }
        }, null!);
        synch.BeginMessageLoop();
        SynchronizationContext.SetSynchronizationContext(oldContext);
        return ret;
    }

    private sealed class ExclusiveSynchronizationContext : SynchronizationContext
    {
        private bool done;
        public Exception? InnerException { get; set; }
        private readonly AutoResetEvent workItemsWaiting = new(false);

        private readonly Queue<Tuple<SendOrPostCallback, object?>> items =
            new();

        public override void Send(SendOrPostCallback d, object? state)
        {
            throw new NotSupportedException("We cannot send to our same thread");
        }

        public override void Post(SendOrPostCallback d, object? state)
        {
            lock (items)
            {
                items.Enqueue(Tuple.Create(d, state));
            }
            workItemsWaiting.Set();
        }

        public void EndMessageLoop()
        {
            Post(_ => done = true, null);
        }

        public void BeginMessageLoop()
        {
            while (!done)
            {
                Tuple<SendOrPostCallback, object?> task = null!;
                lock (items)
                {
                    if (items.Count > 0)
                    {
                        task = items.Dequeue();
                    }
                }
                if (task != null)
                {
                    task.Item1(task.Item2);
                    if (InnerException != null) // the method threw an exeption
                    {
                        throw new AggregateException("AsyncHelpers.Run method threw an exception.", InnerException);
                    }
                }
                else
                {
                    workItemsWaiting.WaitOne();
                }
            }
        }

        public override SynchronizationContext CreateCopy()
        {
            return this;
        }
    }
}