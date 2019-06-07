using System;

namespace Bellight.Core.Misc
{
    public static class StaticLog
    {
        public static IStaticLogProvider Provider { get; set; } = new ConsoleStaticLogProvider();

        public static void Error(string message) {
            Provider.Error(message);
        }

        public static void Error(Exception ex, string messageTemplate) {
            Provider.Error(ex, messageTemplate);
        }
    }
}
