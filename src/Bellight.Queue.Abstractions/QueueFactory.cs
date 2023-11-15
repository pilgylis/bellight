using Microsoft.Extensions.Configuration;
using System;

namespace Bellight.Queue.Abstractions;

public class QueueFactory : IQueueFactory
{
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;

    public QueueFactory(IConfiguration configuration, IServiceProvider serviceProvider)
    {
        _configuration = configuration;
        _serviceProvider = serviceProvider;
    }

    public IQueueService Create()
    {
        var providersSection = _configuration.GetSection("Providers:Queue");
        if (providersSection == null || string.IsNullOrEmpty(providersSection.Value))
        {
            throw new Exception("Missing Providers.Queue configuration");
        }

        var type = Type.GetType(providersSection.Value);
        return _serviceProvider.GetService(type) as IQueueService;
    }
}
