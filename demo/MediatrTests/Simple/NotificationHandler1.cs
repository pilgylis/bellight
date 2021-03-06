﻿using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace MediatrTests.Simple
{
    public class NotificationHandler1 : INotificationHandler<NotificationMessage>
    {
        private readonly IAssertService<NotificationMessage> assertService;

        public NotificationHandler1(IAssertService<NotificationMessage> assertService) {
            this.assertService = assertService;
        }

        public Task Handle(NotificationMessage notification, CancellationToken cancellationToken)
        {
            assertService.Process(notification);
            return Task.CompletedTask;
        }
    }
}
