using System;
using System.Threading.Tasks;

namespace Bellight.Core.Misc
{
    public static class SafeExecute
    {
        /// <summary>
        /// Perform the action with retries and logging
        /// </summary>
        /// <param name="action"></param>
        /// <param name="maxRetries">The maximum number of retries; will retry indefinitely if given a negative value</param>
        /// <param name="millisecondsTimeOut">The interval between retries; not allow a value that is less than 100</param>
        /// <param name="currentRetry">The current retry instance</param>
        /// <returns></returns>
        public static bool Sync(Action action, int maxRetries = 0, int millisecondsTimeOut = 100, int currentRetry = 0)
        {
            try
            {
                action();
                return true;
            }
            catch (Exception ex)
            {
                if (maxRetries < 0 || (currentRetry >= 0 && currentRetry < maxRetries))
                {
                    var timeout = millisecondsTimeOut < 100 ? 100 : millisecondsTimeOut;
                    Task.Delay(TimeSpan.FromMilliseconds(timeout)).Wait();
                    currentRetry++;
                    StaticLog.Error(ex, $"Error occurred. Retrying number {currentRetry}");
                    return Sync(action, maxRetries, millisecondsTimeOut, currentRetry);
                }

                StaticLog.Error(ex, ex.Message);
                return false;
            }
        }

        public static bool SyncCatch(Action action, Action actionCatch, int maxRetries = 0, int millisecondsTimeOut = 100, int currentRetry = 0)
        {
            try
            {
                action();
                return true;
            }
            catch (Exception ex)
            {
                if (maxRetries < 0 || (currentRetry >= 0 && currentRetry < maxRetries))
                {
                    var timeout = millisecondsTimeOut < 100 ? 100 : millisecondsTimeOut;
                    Task.Delay(TimeSpan.FromMilliseconds(timeout)).Wait();
                    currentRetry++;
                    StaticLog.Error(ex, $"Error occurred. Retrying number {currentRetry}");
                    return Sync(action, maxRetries, millisecondsTimeOut, currentRetry);
                }

                StaticLog.Error(ex, ex.Message);
                actionCatch?.Invoke();
                return false;
            }
        }

        /// <summary>
        /// Perform the action with retries and logging
        /// </summary>
        /// <param name="action"></param>
        /// <param name="maxRetries">The maximum number of retries; will retry indefinitely if given a negative value</param>
        /// <param name="millisecondsTimeOut">The interval between retries; not allow a value that is less than 100</param>
        /// <param name="currentRetry">The current retry instance</param>
        /// <returns></returns>
        public static async Task<bool> Async(Func<Task> action, int maxRetries = 0, int millisecondsTimeOut = 100, int currentRetry = 0)
        {
            try
            {
                await action();
                return true;
            }
            catch (Exception ex)
            {
                if (maxRetries < 0 || (currentRetry >= 0 && currentRetry < maxRetries))
                {
                    var timeout = millisecondsTimeOut < 100 ? 100 : millisecondsTimeOut;
                    await Task.Delay(TimeSpan.FromMilliseconds(timeout));
                    currentRetry++;

                    StaticLog.Error(ex, $"Error occurred. Retrying number {currentRetry}");
                    var nextTimeout = timeout * 10;
                    if (nextTimeout > 120 * 1000)
                    {
                        nextTimeout = 120 * 1000;
                    }

                    return await Async(action, maxRetries, nextTimeout, currentRetry);
                }

                StaticLog.Error(ex, ex.Message);
                return false;
            }
        }

        public static async Task<bool> AsyncCatch(Func<Task> action, Action actionCatch, int maxRetries = 0, int millisecondsTimeOut = 100, int currentRetry = 0)
        {
            try
            {
                await action();
                return true;
            }
            catch (Exception ex)
            {
                if (maxRetries < 0 || (currentRetry >= 0 && currentRetry < maxRetries))
                {
                    var timeout = millisecondsTimeOut < 100 ? 100 : millisecondsTimeOut;
                    await Task.Delay(TimeSpan.FromMilliseconds(timeout));
                    currentRetry++;
                    StaticLog.Error(ex, $"Error occurred. Retrying number {currentRetry}");
                    return await Async(action, maxRetries, millisecondsTimeOut, currentRetry);
                }

                StaticLog.Error(ex, ex.Message);
                actionCatch?.Invoke();

                return false;
            }
        }
    }
}
