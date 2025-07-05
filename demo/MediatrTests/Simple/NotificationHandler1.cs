using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace MediatrTests.Simple;

public class NotificationHandler1(IAssertService<NotificationMessage> assertService) : INotificationHandler<NotificationMessage>
{
    public Task Handle(NotificationMessage notification, CancellationToken cancellationToken)
    {
        assertService.Process(notification);
        return Task.CompletedTask;
    }
}