using Bellight.Core.Misc;
using Bellight.MessageBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Threading.Tasks;

namespace MessageBusPublisher;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();
        var topic1 = "test1";
        var messageBusType = MessageBusType.PubSub;

        var typeText = messageBusType == MessageBusType.Queue ? "queue" : "topic";
        Console.WriteLine("Sending messages to {0} '{1}'", typeText, topic1);

        // Settings for Azure Message Bus
        // var policyName = WebUtility.UrlEncode(""); // enter policy name
        // var key = WebUtility.UrlEncode(""); // enter key
        // var namespaceUrl = "";

        // var connectionString = $"amqps://{policyName}:{key}@{namespaceUrl}/";
        //var connectionString = "amqp://artemis:simetraehcapa@localhost:5672";
        var connectionString = "amqps://emsp-mq-admin:buDJr1%7DskF%24wCsn5%3CJnG@b-bab7f2cd-3b7f-4495-a43d-71857b7a6565-1.mq.ap-southeast-1.amazonaws.com:5671";

        var services = new ServiceCollection();
        services.AddLogging(loggingBuilder =>
            loggingBuilder.AddSerilog(dispose: true));

        services.AddBellightMessageBus()
            .AddAmqp(options =>
            {
                options.Endpoint = connectionString;
                options.IsAzureMessageBus = "false";
                options.SubscriberName = "sub1";
            });

        var serviceProvider = services.BuildServiceProvider();

        var message = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
        for (var i = 0; i < 10; i++)
        {
            using var scope = serviceProvider.CreateScope();
            var messageBusFactory = scope.ServiceProvider.GetRequiredService<IMessageBusFactory>();
            var publisher = messageBusFactory.GetPublisher(topic1, messageBusType);
            await publisher.SendAsync(message);
            Console.WriteLine("{0} message(s) sent!", i + 1);
        }

        var topic2 = "test2";

        for (var i = 0; i < 10; i++)
        {
            using var scope = serviceProvider.CreateScope();
            var messageBusFactory = scope.ServiceProvider.GetRequiredService<IMessageBusFactory>();
            var publisher = messageBusFactory.GetPublisher(topic2, messageBusType);
            await publisher.SendAsync(message);
            Console.WriteLine("{0} message(s) sent!", i + 1);
        }
    }

    private static void AcceptInput(IPublisher publisher)
    {
        var acceptInput = true;
        Console.CancelKeyPress += (object? s, ConsoleCancelEventArgs e) =>
        {
            e.Cancel = true;
            acceptInput = false;
            publisher.Dispose();
        };

        Console.WriteLine("Done!");
        while (acceptInput)
        {
            var enter = "[Enter]";
            CoreLogging.Logger?.LogInformation("Enter text and press {Enter} to add message to queue:",
                enter.ToLowerInvariant());
            var text = Console.ReadLine();
            if (string.IsNullOrEmpty(text))
            {
                continue;
            }

            publisher.Send(text);
            CoreLogging.Logger?.LogInformation("Message {Text} sent!", text);
        }
    }
}