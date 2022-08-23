using Bellight.MongoDb.Transactions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;
using System.Security.Authentication;

namespace Bellight.MongoDb;

public class CollectionFactory : ICollectionFactory
{
    private readonly MongoDbSettings _settings;
    private readonly IServiceProvider serviceProvider;
    private IMongoDatabase? _database;

    private ILogger<CollectionFactory>? logger;
    private bool loggerResolved = false;

    protected ILogger<CollectionFactory>? Logger
    {
        get
        {
            if (loggerResolved)
            {
                return logger;
            }

            loggerResolved = true;

            logger = serviceProvider.GetRequiredService<ILogger<CollectionFactory>>();

            return logger;
        }
    }

    public IMongoDatabase Database
    {
        get
        {
            if (_database != null) return _database;
            var client = CreateClient();
            _database = client.GetDatabase(_settings.DatabaseName);

            return _database;
        }
    }
    public CollectionFactory(IOptions<MongoDbSettings> optionsAccessor, IServiceProvider serviceProvider)
    {
        _settings = optionsAccessor.Value;
        this.serviceProvider = serviceProvider;
    }

    public IMongoCollection<T> GetCollection<T>(string collectionName) where T : class
    {
        return Database.GetCollection<T>(collectionName);
    }

    private IMongoClient CreateClient()
    {
        var url = new MongoUrl(_settings.ConnectionString);
        var clientSettings = MongoClientSettings.FromUrl(url);

        if ("true".Equals(_settings.UseSsl, StringComparison.OrdinalIgnoreCase))
        {
            clientSettings.SslSettings = new SslSettings()
            {
                EnabledSslProtocols = SslProtocols.Tls12
            };
        }

        if ("true".Equals(_settings.LogQuery, StringComparison.OrdinalIgnoreCase))
        {
            clientSettings.ClusterConfigurator = cb =>
            {
                cb.Subscribe<CommandStartedEvent>(e =>
                    Logger?.LogInformation("Executing query: {commandName} - {command}",
                        e.CommandName,
                        e.Command));
            };
        }

        var client = new MongoClient(clientSettings);

        return client.AsTransactionClient();
    }
}
