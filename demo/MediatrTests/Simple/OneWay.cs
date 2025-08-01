﻿using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace MediatrTests.Simple;

public class OneWay : IRequest
{ }

public class OneWayHandlerWithBaseClass(IAssertService<OneWay> assertService) : IRequestHandler<OneWay>
{
    public Task Handle(OneWay request, CancellationToken cancellationToken)
    {
        assertService.Process(request);
        return Task.CompletedTask;
    }
}