using System;
using System.Threading.Tasks;
using Bellight.Core.Misc;
using Bellight.Queue.Abstractions;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Bellight.Queue.KafkaProvider;

public class KafkaQueueService : QueueServiceBase<IConsumer<Ignore, string>>
{

    private Lazy<IProducer<Null, string>> _producer;
    private Lazy<IConsumer<Ignore, string>> _consumer;

    public KafkaQueueService(
        IConfiguration configuration,
        ISerializer serializer,
        IOptions<QueueOptions> options
    ) : base(serializer, options)
    {
        _producer = new Lazy<IProducer<Null, string>>(() => {
            var config = new ProducerConfig
            {
                BootstrapServers = configuration.GetSection(Constants.OptionNameBootstrapServer).Value,
                BatchNumMessages = 1,
                MessageTimeoutMs = 1,
                SocketNagleDisable = true,
                Acks = Acks.Leader
            };

            return new ProducerBuilder<Null, string>(config).Build();
        });

        _consumer = new Lazy<IConsumer<Ignore, string>>(() => {
            var groupIdSection = configuration.GetSection(Constants.OptionNameConsumerGroupId);
            var groupId = groupIdSection.Exists() && !string.IsNullOrEmpty(groupIdSection.Value)
                ? groupIdSection.Value : Constants.DefaultGroupId;
            var config = new ConsumerConfig
            {
                BootstrapServers = configuration.GetSection(Constants.OptionNameBootstrapServer).Value,
                GroupId = groupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                StatisticsIntervalMs = 5000,
                SessionTimeoutMs = 6000
            };

            var consumer = new ConsumerBuilder<Ignore, string>(config)
                .SetErrorHandler((_, e) => StaticLog.Error($"Receiving Error: {e.Code} - {e.Reason}"))
                .Build();
            return consumer;
        });
    }

    public override void Dispose()
    {
        if (_producer.IsValueCreated)
        {
            var producer = _producer.Value;
            producer.Flush(TimeSpan.FromSeconds(1));
            producer.Dispose();
        }

        if (_consumer.IsValueCreated)
        {
            var consumer = _consumer.Value;
            consumer.Close();
            consumer.Dispose();
        }
    }

    private void Handler(DeliveryReport<Null, string> result)
    {
        if (result?.Error?.IsError == false)
        {
            return;
        }

        StaticLog.Error($"Delivery Error: {result.Error.Code} - {result.Error.Reason}");
    }

    protected override IConsumer<Ignore, string> CreateConsumer(string topic)
    {
        var consumer = _consumer.Value;
        consumer.Subscribe(topic);
        return consumer;
    }

    protected override Task<string> PollMessage(IConsumer<Ignore, string> consumer, int pollingInterval)
    {
        var consumeResult = consumer.Consume(TimeSpan.FromMilliseconds(pollingInterval));
        if (consumeResult == null)
        {
            return Task.FromResult(string.Empty);
        }

        var messageText = consumeResult.Value;

        try
        {
            consumer.Commit(consumeResult);
        }
        catch (KafkaException e)
        {
            Console.WriteLine($"Commit error: {e.Error.Code} - {e.Error.Reason}");
        }

        return Task.FromResult(messageText);
    }

    protected override Task EnqueueAsync(string topic, string message)
    {
        var producer = _producer.Value;
        producer.Produce(topic, new Message<Null, string>
        {
            Value = message
        }, Handler);

        return Task.CompletedTask;
    }
}