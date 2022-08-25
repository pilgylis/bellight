using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace MediatrTests.Simple
{
    public class OneWay : IRequest
    { }

    public class OneWayHandlerWithBaseClass : AsyncRequestHandler<OneWay>
    {
        private readonly IAssertService<OneWay> assertService;

        public OneWayHandlerWithBaseClass(IAssertService<OneWay> assertService)
        {
            this.assertService = assertService;
        }

        protected override Task Handle(OneWay request, CancellationToken cancellationToken)
        {
            assertService.Process(request);
            return Task.CompletedTask;
        }
    }
}