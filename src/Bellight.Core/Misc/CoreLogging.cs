using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bellight.Core.Misc
{
    public static class CoreLogging
    {
        private static IServiceProvider? serviceProvider;
        private static ILogger? logger;
        private static bool loggerSet = false;

        public static ILogger? Logger
        {
            get
            {
                if (!loggerSet && serviceProvider != null)
                {
                    var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
                    logger = loggerFactory.CreateLogger("Bellight.Core");
                    loggerSet = true;
                }

                if (logger == null)
                {
                    logger = InitializeConsoleLogger();
                }

                return logger;
            }
        }
        public static IServiceProvider ConfigureCoreLogging(this IServiceProvider serviceProvider)
        {
            CoreLogging.serviceProvider = serviceProvider;
            return serviceProvider;
        }

        private static ILogger InitializeConsoleLogger()
        {
            var loggerFactory = LoggerFactory.Create(builder => {
                builder
                .AddFilter("Microsoft", LogLevel.Information)
                .AddFilter("System", LogLevel.Information)
                .AddConsole();
            });

            return loggerFactory.CreateLogger("Bellight.Core");
        }
    }
}
