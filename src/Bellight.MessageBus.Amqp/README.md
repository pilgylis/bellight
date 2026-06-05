# Bellight.MessageBus.Amqp

AMQP implementation of [`Bellight.MessageBus.Abstractions`](../Bellight.MessageBus.Abstractions/README.md) backed by [AMQPNetLite](https://github.com/Azure/amqpnetlite). Supports RabbitMQ and Azure Service Bus.

This package provides the transport. Application code depends only on `IMessageBusFactory` from the abstractions package — not on anything in this package.

---

## Installation

```
dotnet add package Bellight.MessageBus.Amqp
```

---

## Registration

```csharp
services.AddBellightMessageBus()
    .AddAmqp(opts =>
    {
        opts.Endpoint       = "amqp://guest:guest@localhost:5672";
        opts.InstanceName   = "my-service";   // optional topic prefix
        opts.SubscriberName = "consumer-1";   // unique per consumer process
    });
```

`AddAmqp` registers:
- `AmqpQueueProvider` as `IQueueProvider` (transient)
- `AmqpPubsubProvider` as `IPubsubProvider` (transient)
- `AmqpConnectionFactory` as `IAmqpConnectionFactory` (singleton)
- `AmqpAddressBuilders` (singleton, customisable — see below)

---

## Configuration options

All properties on `AmqpOptions`:

| Property | Type | Default | Description |
|---|---|---|---|
| `Endpoint` | `string` | — | AMQP broker URL, e.g. `amqp://user:pass@host:5672` |
| `InstanceName` | `string` | — | Prepended to every topic as `{InstanceName}.{topic}`. Omit to use the bare topic name. |
| `SubscriberName` | `string` | — | Unique name for this consumer process. Used to build the subscriber queue address. |
| `IsAzureMessageBus` | `string` | — | Set to `"true"` to use Azure Service Bus subscription address format for PubSub. |
| `PollingIntervalMilliseconds` | `int` | `2000` | How long `ReceiveAsync` waits for a message before returning null. |
| `WaitDurationMilliseconds` | `int` | `2000` | Delay between poll cycles when no message is received. |
| `ReceiverQueueIntervalInSeconds` | `int` | `1` | Internal receiver queue interval. |

---

## Address format

Topics are normalised before address construction. If `InstanceName` is set, the effective topic is `{InstanceName}.{topic}`; otherwise it is the bare topic string.

### RabbitMQ (default)

| Mode | Side | Address |
|---|---|---|
| `Queue` | Publisher | `/queues/{topic}` |
| `Queue` | Subscriber | `/queues/{topic}` |
| `PubSub` | Publisher | `/exchanges/{topic}` |
| `PubSub` | Subscriber | `/queues/{topic}.{SubscriberName}` |

For PubSub the publisher writes to a **fanout exchange**. Each subscriber consumes from its own **dedicated queue** bound to that exchange, so every subscriber receives every message.

### Azure Service Bus (`IsAzureMessageBus = "true"`)

| Mode | Side | Address |
|---|---|---|
| `PubSub` | Subscriber | `{topic}/Subscriptions/{SubscriberName}` |

Queue mode address is unchanged.

---

## Publishing

```csharp
public class OrderService(IMessageBusFactory bus)
{
    public async Task EnqueueAsync(string payload)
    {
        IPublisher publisher = bus.GetPublisher("orders", MessageBusType.Queue);
        await publisher.SendAsync(payload);
    }

    public async Task BroadcastAsync(string payload)
    {
        IPublisher publisher = bus.GetPublisher("order-events", MessageBusType.PubSub);
        await publisher.SendAsync(payload);
    }
}
```

`GetPublisher` is thread-safe and caches publishers by `(topic, MessageBusType)`.

---

## Subscribing

```csharp
public class OrderProcessor(IMessageBusFactory bus) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ISubscription queueSub = bus.Subscribe(
            topic: "orders",
            messageReceivedAction: async msg => await HandleOrderAsync(msg),
            messageBusType: MessageBusType.Queue);

        ISubscription pubsubSub = bus.Subscribe(
            topic: "order-events",
            messageReceivedAction: async msg => await HandleEventAsync(msg),
            messageBusType: MessageBusType.PubSub);

        stoppingToken.Register(() =>
        {
            queueSub.Dispose();
            pubsubSub.Dispose();
        });

        return Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
```

Each `Subscribe` call starts a background polling loop. Disposing the returned `ISubscription` cancels the loop.

---

## Polling and resilience

The subscriber uses a polling model:

1. Calls `ReceiveAsync` with a timeout of `PollingIntervalMilliseconds`.
2. If a message arrives, accepts it and invokes the handler.
3. If no message arrives within the timeout, waits `WaitDurationMilliseconds` before the next poll.
4. If an exception is thrown, logs the error, waits `WaitDurationMilliseconds`, and resumes polling.
5. AMQP links and the connection are lazily re-established if found closed at the start of any poll cycle.

There is no dead-letter or retry logic in this package — that must be handled inside the message handler or at the broker level.

---

## Customising address construction

Pass an `Action<AmqpAddressBuilders>` as the second argument to `AddAmqp` to override either address delegate:

```csharp
services.AddBellightMessageBus()
    .AddAmqp(
        opts => { opts.Endpoint = "amqp://localhost:5672"; },
        builders =>
        {
            // custom publisher address
            builders.PublisherAddress = (topic, type) =>
                type == MessageBusType.Queue ? $"q/{topic}" : $"ex/{topic}";

            // custom subscriber address
            builders.SubscriberAddress = (topic, type, opts) =>
                $"q/{topic}/{opts.SubscriberName}";
        });
```

---

## RabbitMQ setup

For PubSub to work, the broker must have:
- A **fanout exchange** named `{topic}` (or `{InstanceName}.{topic}`)
- A **durable queue** per subscriber named `{topic}.{SubscriberName}`
- A **binding** from the exchange to each subscriber queue

Example RabbitMQ definitions for a topic `pubsub-demo` with subscriber `sub1`:

```json
{
  "queues": [
    { "name": "pubsub-demo.sub1", "vhost": "/", "durable": true, "auto_delete": false }
  ],
  "exchanges": [
    { "name": "pubsub-demo", "vhost": "/", "type": "fanout", "durable": true, "auto_delete": false }
  ],
  "bindings": [
    {
      "source": "pubsub-demo", "vhost": "/",
      "destination": "pubsub-demo.sub1", "destination_type": "queue",
      "routing_key": ""
    }
  ]
}
```

Queue mode requires only a durable queue named `{topic}`. No exchange or binding is needed.

---

## ASP.NET Core example

```csharp
// Program.cs
builder.Services.AddBellightMessageBus()
    .AddAmqp(opts =>
    {
        opts.Endpoint = builder.Configuration["ServiceBus:ConnectionString"];
        opts.SubscriberName = "api-1";
    });
```

```csharp
// Controller
[HttpPost]
public async Task<IActionResult> Publish([FromBody] string message)
{
    var publisher = _bus.GetPublisher("notifications", MessageBusType.PubSub);
    await publisher.SendAsync(message);
    return Ok();
}
```
