using Bellight.MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;

namespace MediatrTests.Simple;

public class ServiceProviderFixture : IDisposable
{
    public IServiceProvider ServiceProvider { get; }
    public Mock<IAssertService<OneWay>> AssertOneWay { get; }
    public Mock<IAssertService<NotificationMessage>> AssertNotification { get; }

    private readonly IServiceScope scope;

    public ServiceProviderFixture()
    {
        var services = new ServiceCollection();

        services.AddBellightCore(options =>
        {
            options.DependencyCacheOptions.PrettyPrint = true;
            options.DependencyCacheOptions.Enabled = false;

            options.AddMediatR(cfg => { });
        });

        AssertOneWay = new Mock<IAssertService<OneWay>>();
        services.AddSingleton(AssertOneWay.Object);

        AssertNotification = new Mock<IAssertService<NotificationMessage>>();
        services.AddSingleton(AssertNotification.Object);

        scope = services.BuildServiceProvider().CreateScope();
        ServiceProvider = scope.ServiceProvider;
    }

    public void Dispose()
    {
        scope?.Dispose();
    }
}