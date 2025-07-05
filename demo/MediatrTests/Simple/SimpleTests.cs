using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace MediatrTests.Simple;

public class SimpleTests(ServiceProviderFixture fixture) : IClassFixture<ServiceProviderFixture>
{
    [Fact]
    public async Task PingPongTest()
    {
        var mediator = fixture.ServiceProvider.GetService<IMediator>();

        var response = await mediator.Send(new Ping());

        Assert.Equal("Pong", response);
    }

    [Fact]
    public async Task OneWayTest()
    {
        var mediator = fixture.ServiceProvider.GetService<IMediator>();

        await mediator.Send(new OneWay());

        fixture.AssertOneWay.Verify(x => x.Process(It.IsAny<OneWay>()), Times.Once);
    }

    [Fact]
    public async Task NotificationTest()
    {
        var mediator = fixture.ServiceProvider.GetService<IMediator>();
        await mediator.Publish(new NotificationMessage());

        fixture.AssertNotification.Verify(x => x.Process(It.IsAny<NotificationMessage>()), Times.Exactly(2));
    }
}