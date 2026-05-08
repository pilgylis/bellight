using Bellight.MessageBus.Abstractions;

var builder = WebApplication.CreateBuilder(args);

var topic = "test1";
var messageBusType = MessageBusType.PubSub;

var subscriberName = "sub1";

var typeText = messageBusType == MessageBusType.Queue ? "queue" : "topic";
Console.WriteLine($"Subscribing {typeText} '{topic}'");

// Settings for Azure Message Bus
// var policyName = WebUtility.UrlEncode(""); // enter policy name
// var key = WebUtility.UrlEncode(""); // enter key
// var namespaceUrl = "";

// var connectionString = $"amqps://{policyName}:{key}@{namespaceUrl}/";

var connectionString = "amqp://artemis:simetraehcapa@localhost:5672";

Console.WriteLine(connectionString);

builder.Services.AddBellightMessageBus()
    .AddAmqp(options =>
    {
        options.Endpoint = connectionString;
        options.IsAzureMessageBus = "false";
        options.SubscriberName = subscriberName;
    });

var app = builder.Build();
var serviceProvider = app.Services;

var messageBusFactory = serviceProvider.GetRequiredService<IMessageBusFactory>();

var subscription = messageBusFactory.Subscribe(topic, OnMessageReceived, messageBusType);
Console.ReadLine();
subscription.Dispose();

static Task OnMessageReceived(string message)
{
    Console.WriteLine(message);
    return Task.CompletedTask;
}