using Aspire.Hosting;
using Bellight.DataManagement;
using Bellight.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkTests;

public class EntityFrameworkFixture : IDisposable
{
    private IServiceProvider? _serviceProvider;

    private string _connectionString;
    private DistributedApplication? _app;

    public EntityFrameworkFixture()
    {
        _ = Init();
    }

    private async Task Init()
    {
        var defaultTimeout = TimeSpan.FromSeconds(30);
        var cancellationToken = new CancellationTokenSource(defaultTimeout).Token;
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.PostGreAppHost>();
        _app = await appHost.BuildAsync(cancellationToken).WaitAsync(defaultTimeout, cancellationToken);

        var resourceNotificationService = _app.Services.GetRequiredService<ResourceNotificationService>();
        await _app.StartAsync();

        await resourceNotificationService.WaitForResourceHealthyAsync("postgres", cancellationToken);

        _connectionString = await _app.GetConnectionStringAsync("postgres", cancellationToken) ?? string.Empty;

        var services = new ServiceCollection();
        services.AddDbContext<TestingDbContext>(dbContextOptionsBuilder =>
        {
            dbContextOptionsBuilder.UseNpgsql(_connectionString);
        });

        services.AddSingleton<DbContext>(p => p.GetRequiredService<TestingDbContext>());

        services.AddTransient(typeof(IRepository<,>), typeof(EntityFrameworkRepository<,>));

        var serviceProvider = services.BuildServiceProvider();

        var context = serviceProvider.GetRequiredService<TestingDbContext>();
        await context.Database.EnsureCreatedAsync();

        _serviceProvider = serviceProvider;
    }

    public async Task<IServiceProvider> GetServiceProviderAsync()
    {
        if (_serviceProvider is not null)
        {
            return _serviceProvider;
        }

        while (_serviceProvider is null)
        {
            await Task.Delay(100);
        }

        return _serviceProvider.CreateScope().ServiceProvider;
    }

    public async Task<string> GetConnectionStringAsync()
    {
        if (_connectionString is not null)
        {
            return _connectionString;
        }

        while (_connectionString is null)
        {
            await Task.Delay(100);
        }

        return _connectionString;
    }

    
    public void Dispose()
    {
        _app?.Dispose();
    }
}
