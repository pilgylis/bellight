using Bellight.MessageBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace MessageBusPublisher
{
    static class Program
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
            Console.WriteLine($"Sending messages to {typeText} '{topic}'");

            var services = new ServiceCollection();
            services.AddBellightMessageBus()
                .AddAmqp(options => options.Endpoint = "amqp://artemis:simetraehcapa@localhost:5672");

            var serviceProvider = services.BuildServiceProvider();

            var messageBusFactory = serviceProvider.GetService<IMessageBusFactory>();
            var publisher = messageBusFactory.GetPublisher(topic, messageBusType);

            var acceptInput = true;
            Console.CancelKeyPress += (object s, ConsoleCancelEventArgs e) =>
            {
                e.Cancel = true;
                acceptInput = false;
                publisher.Dispose();
            };

            Console.WriteLine("Done!");
            while (acceptInput)
            {
                Console.WriteLine("Enter text and press [Enter] to add message to queue:");
                var text = Console.ReadLine();
                publisher.Send(text);
                Console.WriteLine("Message sent!");
            }
        }
    }
}
