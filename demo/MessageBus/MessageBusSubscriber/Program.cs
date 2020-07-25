using Bellight.MessageBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;

namespace MessageBusSubscriber
{
    class Program
    {
        static void Main(string[] args)
        {
            var topic = args.Length < 2 || string.IsNullOrEmpty(args[1]) ? "bellight.q1" : args[1];
            var messageBusType =
                        args.Length >= 1
                        && !string.IsNullOrEmpty(args[0])
                        && args[0].StartsWith("p", StringComparison.InvariantCultureIgnoreCase)
                    ? MessageBusType.PubSub : MessageBusType.Queue;

            var subscriberName = args.Length < 2 ? "" : args[2];

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
            services.AddBellightMessageBus()
                .AddAmqp(options => {
                    options.Endpoint = connectionString;
                    options.IsAzureMessageBus = "true";
                    options.SubscriberName = subscriberName;
                });

            var serviceProvider = services.BuildServiceProvider();

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
