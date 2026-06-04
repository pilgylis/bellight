using Bellight.Core.Misc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.ConfigureStandardLogging();

builder.Services.AddBellightMessageBus()
    .AddAmqp(options =>
    {
        options.Endpoint = builder.Configuration.GetConnectionString("rabbitmq");
        options.IsAzureMessageBus = "false";
        options.SubscriberName = "sub1";
    });

builder.Services.AddHostedService<SubscriberWorker>();

await builder.Build().RunAsync();
