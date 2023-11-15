namespace Bellight.MessageBus.Abstractions;

public interface IPublisher : IDisposable
{
    void Send(string message);

    Task SendAsync(string message);
}