namespace Bellight.MessageBus.Abstractions;

public static class Constants
{
    public static readonly TimeSpan DefaultPollingInterval = TimeSpan.FromSeconds(2);
    public static readonly TimeSpan DefaultWaitDuration = TimeSpan.FromSeconds(2);
}