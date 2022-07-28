﻿using Bellight.DataManagement;
using Microsoft.Extensions.DependencyInjection;

namespace Bellight.MongoDb;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMongoDb(this IServiceCollection services, Action<MongoDbSettings> settingsConfigure)
    {
        services.AddTransaction();
        services.AddSingleton<ICollectionFactory, CollectionFactory>();
        services.AddTransient(typeof(IRepository<,>), typeof(MongoRepository<,>));
        services.AddTransient(typeof(IMongoRepository<,>), typeof(MongoRepository<,>));

        services.Configure(settingsConfigure);
        return services;
    }
}
