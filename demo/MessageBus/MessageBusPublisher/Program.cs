var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddBellightMessageBus()
    .AddAmqp(options =>
    {
        options.Endpoint = builder.Configuration.GetConnectionString("rabbitmq");
        options.IsAzureMessageBus = "false";
        options.SubscriberName = "sub1";
    });

builder.Services.AddHostedService<PublisherWorker>();

await builder.Build().RunAsync();
