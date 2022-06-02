using Bellight.Core.Misc;
using Bellight.MessageBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Net;

namespace MessageBusSubscriber
{
    class Program
    {
        static void Main(string[] args)
        {
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
            var services = new ServiceCollection();
            services.AddLogging(loggingBuilder =>
                loggingBuilder.AddSerilog(dispose: true));

            services.AddBellightMessageBus()
                .AddAmqp(options => {
                    options.Endpoint = connectionString;
                    options.IsAzureMessageBus = "false";
                    options.SubscriberName = subscriberName;
                });

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.ConfigureCoreLogging();

            var messageBusFactory = serviceProvider.GetService<IMessageBusFactory>();

            var subscription = messageBusFactory.Subscribe(topic, OnMessageReceived, messageBusType);
            Console.ReadLine();
            subscription.Dispose();
        }

        static void OnMessageReceived(string message)
        {
            Console.WriteLine(message);
        }
    }
}
