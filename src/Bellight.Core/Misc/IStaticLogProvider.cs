using System;

namespace Bellight.Core.Misc
{
    public interface IStaticLogProvider
    {
        void Error(string message);
        void Error(Exception ex, string messageTemplate);
        void Warning(string message);
        void Information(string message);
    }
}
