# Bellight.MessageBus.Abstractions

Provider-agnostic abstraction layer for message queuing and publish/subscribe messaging in .NET 10.

This package defines the contracts and the factory that application code programs against. It contains **no transport logic**. A concrete implementation package (e.g. `Bellight.MessageBus.Amqp`) must be registered at startup to handle the actual message delivery.

---

## Concepts

### Message bus types

| `MessageBusType` | Pattern | Delivery |
|---|---|---|
| `Queue` | Point-to-point | Exactly one consumer receives each message |
| `PubSub` | Publish/Subscribe | Every subscriber receives every message |

### Core interfaces

| Interface | Role |
|---|---|
| `IMessageBusFactory` | Entry point — resolves publishers and subscriptions |
| `IPublisher` | Sends a string message to a named topic |
| `ISubscription` | Represents an active subscription; dispose to cancel |
| `IQueueProvider` | Implement this to provide a Queue transport |
| `IPubsubProvider` | Implement this to provide a PubSub transport |

---

## Installation

```
dotnet add package Bellight.MessageBus.Abstractions
```

---

## Registration

```csharp
services.AddBellightMessageBus();
```

`AddBellightMessageBus()` registers `IMessageBusFactory` as a singleton and returns a `MessageBusBuilder` for chaining provider registrations.

### Registering a provider package

Provider packages (e.g. `Bellight.MessageBus.Amqp`) extend `MessageBusBuilder` with their own fluent method:

```csharp
services.AddBellightMessageBus()
    .AddAmqp(opts => { opts.Endpoint = "amqp://localhost:5672"; });
```

### Registering a custom provider directly

```csharp
services.AddBellightMessageBus()
    .AddQueueProvider<MyQueueProvider>()
    .AddPubsubProvider<MyPubsubProvider>();
```

---

## Usage

Inject `IMessageBusFactory` wherever you need to publish or subscribe.

### Publishing

```csharp
public class OrderService(IMessageBusFactory bus)
{
    public async Task PlaceOrderAsync(string payload)
    {
        IPublisher publisher = bus.GetPublisher("orders", MessageBusType.Queue);
        await publisher.SendAsync(payload);
    }
}
```

`GetPublisher` is thread-safe and caches publisher instances by `(topic, MessageBusType)`, so it is safe to call on every request.

### Subscribing

```csharp
public class OrderProcessor(IMessageBusFactory bus) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ISubscription subscription = bus.Subscribe(
            topic: "orders",
            messageReceivedAction: async message =>
            {
                // handle message
                await ProcessAsync(message);
            },
            messageBusType: MessageBusType.Queue);

        stoppingToken.Register(() => subscription.Dispose());
        return Task.CompletedTask;
    }
}
```

Disposing the `ISubscription` cancels the consumer loop inside the transport implementation.

---

## Implementing a custom provider

Implement `IQueueProvider`, `IPubsubProvider`, or both. Each interface inherits from `IMessageBusProvider`:

```csharp
public interface IMessageBusProvider
{
    IPublisher    GetPublisher(string topic);
    ISubscription Subscribe(string topic, Func<string, Task> messageReceivedAction);
}
```

Minimal skeleton:

```csharp
public class InMemoryQueueProvider : IQueueProvider
{
    public IPublisher GetPublisher(string topic)
        => new InMemoryPublisher(topic);

    public ISubscription Subscribe(string topic, Func<string, Task> handler)
    {
        var cts = new CancellationTokenSource();
        // start consumer loop, pass cts.Token
        return new DefaultSubscription(cts);
    }
}
```

`DefaultSubscription` is provided by this package. It wraps a `CancellationTokenSource` and cancels it on `Dispose`.

---

## Provider resolution

`MessageBusFactory` resolves providers in this order:

1. **DI container** — looks for a registered `IQueueProvider` or `IPubsubProvider`.
2. **Configuration fallback** — if DI returns nothing, reads a type name from `IConfiguration`:

| Bus type | Configuration key |
|---|---|
| `Queue` | `Providers:MessageBusQueue` |
| `PubSub` | `Providers:MessageBusPubsub` |

The configuration value must be an assembly-qualified type name:

```json
{
  "Providers": {
    "MessageBusQueue": "My.Namespace.InMemoryQueueProvider, My.Assembly"
  }
}
```

If neither source returns a provider, `MessageBusProviderNotFoundException` is thrown.

---

## Exceptions

| Type | Thrown when |
|---|---|
| `MessageBusProviderNotFoundException` | No provider is found for the requested `MessageBusType` |
| `MessageBusException` | The resolved provider returns a null publisher |

---

## Constants

```csharp
Constants.DefaultPollingInterval  // TimeSpan.FromSeconds(2)
Constants.DefaultWaitDuration     // TimeSpan.FromSeconds(2)
```

Transport implementations should use these as their default polling and reconnect-wait values.
