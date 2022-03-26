using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bellight.Core.Misc
{
    public static class CoreLogging
    {
        private static IServiceProvider? serviceProvider;
        private static ILogger? logger;
        public static ILogger? Logger
        {
            get
            {
                if (logger == null && serviceProvider != null)
                {
                    var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
                    logger = loggerFactory.CreateLogger("Bellight.Core");
                }

                return logger;
            }
        }
        public static void SetServiceProvider(IServiceProvider serviceProvider)
        {
            CoreLogging.serviceProvider = serviceProvider;
        }
    }
}
