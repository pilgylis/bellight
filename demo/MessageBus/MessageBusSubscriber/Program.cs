using Bellight.MessageBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace MessageBusSubscriber
{
    class Program
    {
        static void Main(string[] args)
        {
            var topic = args.Length < 2 || string.IsNullOrEmpty(args[1]) ? "q1" : args[1];
            var messageBusType =
                        args.Length >= 1
                        && !string.IsNullOrEmpty(args[0])
                        && args[0].StartsWith("p", StringComparison.InvariantCultureIgnoreCase)
                    ? MessageBusType.PubSub : MessageBusType.Queue;

            var typeText = messageBusType == MessageBusType.Queue ? "queue" : "topic";
            Console.WriteLine($"Subscribing {typeText} '{topic}'");

            var services = new ServiceCollection();
            services.AddBellightMessageBus()
                .AddAmqp(options => options.Endpoint = "amqp://admin:Abc%40123@localhost:5672");

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
