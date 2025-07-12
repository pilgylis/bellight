using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Bellight.MongoDb;
using MartinCostello.Logging.XUnit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace MongoDbTests;

public class MongoDbFixture: ITestOutputHelperAccessor
{
    private IServiceProvider? _serviceProvider;

    private string? _connectionString;
    private DistributedApplication? _app;

    public ITestOutputHelper? OutputHelper { get; set; }

    public MongoDbFixture()
    {
        _ = Init();
    }

    private async Task Init()
    {
        var defaultTimeout = TimeSpan.FromSeconds(30);
        var cancellationToken = new CancellationTokenSource(defaultTimeout).Token;
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.MongoDbAppHost>();
        _app = await appHost.BuildAsync(cancellationToken).WaitAsync(defaultTimeout, cancellationToken);

        await _app.StartAsync();

        do
        {
            await Task.Delay(100).ConfigureAwait(false);

            _connectionString = await _app.GetConnectionStringAsync("mongo", cancellationToken);
        } while (_connectionString is null);

        var services = new ServiceCollection();
        services.AddLogging((builder) => builder.AddXUnit(this));
        services.AddMongoDb(options =>
       {
           options.ConnectionString = _connectionString;
           options.DatabaseName = "mongodb";
           options.LogQuery = "true";
       });

        var serviceProvider = services.BuildServiceProvider();
        _serviceProvider = serviceProvider;
    }

    public async Task<IServiceProvider> GetServiceProviderAsync(ITestOutputHelper? testOutputHelper = null)
    {
        OutputHelper = testOutputHelper;

        if (_serviceProvider is not null)
        {
            return _serviceProvider;
        }

        while (_serviceProvider is null)
        {
            await Task.Delay(100);
        }

        return _serviceProvider;
    }

    public async Task<string> GetConnectionStringAsync()
    {
        if (_connectionString is not null)
        {
            return _connectionString;
        }

        while (_connectionString is null)
        {
            await Task.Delay(100).ConfigureAwait(false);
        }

        return _connectionString;
    }

    public void Dispose()
    {
        _app?.Dispose();
    }
}