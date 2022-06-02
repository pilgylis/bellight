using Bellight.Core.Misc;
using Bellight.MessageBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Net;

namespace MessageBusPublisher
{
    static class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
              .Enrich.FromLogContext()
              .WriteTo.Console()
              .CreateLogger();
            var topic = "test1";
            var messageBusType = MessageBusType.PubSub;

            var typeText = messageBusType == MessageBusType.Queue ? "queue" : "topic";
            Console.WriteLine("Sending messages to {0} '{1}'", typeText, topic);

            // Settings for Azure Message Bus
            // var policyName = WebUtility.UrlEncode(""); // enter policy name
            // var key = WebUtility.UrlEncode(""); // enter key
            // var namespaceUrl = ""; 

            // var connectionString = $"amqps://{policyName}:{key}@{namespaceUrl}/";
            var connectionString = "amqp://artemis:simetraehcapa@localhost:5672";
            
            var services = new ServiceCollection();
            services.AddLogging(loggingBuilder =>
                loggingBuilder.AddSerilog(dispose: true));

            services.AddBellightMessageBus()
                .AddAmqp(options => {
                    options.Endpoint = connectionString;
                    options.IsAzureMessageBus = "false";
                    options.SubscriberName = "sub1";
                });

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.ConfigureCoreLogging();

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
                var enter = "[Enter]";
                CoreLogging.Logger?.LogInformation("Enter text and press {enter} to add message to queue:", 
                    enter.ToLowerInvariant());
                var text = Console.ReadLine();
                publisher.Send(text);
                CoreLogging.Logger?.LogInformation("Message {text} sent!", text);
            }
        }
    }
}
