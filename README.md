# Bellight

A set of .NET 10 libraries that provide provider-agnostic abstractions for data repositories and message buses, with concrete implementations for Entity Framework Core, MongoDB, and AMQP (RabbitMQ).

## Packages

| Package | Purpose |
|---|---|
| `Bellight.DataManagement` | Core `IRepository<T, TKey>` abstraction and entity contracts |
| `Bellight.EntityFrameworkCore` | EF Core implementation of `IRepository<T, TKey>` |
| `Bellight.MongoDb` | MongoDB implementation of `IRepository<T, TKey>` |
| `Bellight.MessageBus.Abstractions` | Core `IPublisher` / `ISubscriber` abstraction |
| `Bellight.MessageBus.Amqp` | AMQP (RabbitMQ) implementation of the message bus |

---

## Data Management

### Core Concepts

**`IEntity<TKey>`** — base entity contract every persisted object must implement:

```csharp
public interface IEntity<TKey>
{
    TKey Id { get; set; }
    bool IsDeleted { get; set; }   // soft-delete flag
}
```

**`IRepository<T, TKey>`** — generic async CRUD interface:

```csharp
// Write
Task AddAsync(T item);
Task AddManyAsync(IEnumerable<T> items);
Task UpdateAsync(TKey id, Func<IEntityUpdateDefinition<T, TKey>, IEntityUpdateDefinition<T, TKey>> update);
Task<long> UpdateManyAsync(Expression<Func<T, bool>> filter, Func<IEntityUpdateDefinition<T, TKey>, IEntityUpdateDefinition<T, TKey>> update);
Task ReplaceAsync(TKey id, T item);
Task DeleteAsync(TKey id, bool softDelete = true);
Task<long> DeleteManyAsync(Expression<Func<T, bool>> filter, bool softDelete = true);

// Read
Task<T?> GetByIdAsync(TKey id);
Task<IEnumerable<T>> FindAsync(
    Expression<Func<T, bool>> filter,
    Expression<Func<IEntitySortDefinition<T, TKey>, IEntitySortDefinition<T, TKey>>>? sortOrders,
    int pageIndex,
    int pageSize);
Task<IEnumerable<P>> FindAsync<P>(
    Expression<Func<T, bool>> filter,
    Expression<Func<T, P>> projection,
    Expression<Func<IEntitySortDefinition<T, TKey>, IEntitySortDefinition<T, TKey>>>? sortOrders,
    int pageIndex,
    int pageSize);
Task<long> CountAsync(Expression<Func<T, bool>> filter);
Task<bool> Exists(Expression<Func<T, bool>> filter);
IQueryable<T> ToQueryable();   // includes soft-deleted records
```

> All `FindAsync` overloads exclude soft-deleted records automatically. Use `ToQueryable()` for full access.

---

### Entity Framework Core

#### Installation

```
dotnet add package Bellight.EntityFrameworkCore
```

#### Registration

```csharp
services.AddEntityFrameworkContext<AppDbContext>(opts =>
    opts.UseNpgsql(connectionString))          // or UseSqlServer, UseSqlite, etc.
    .AddRepository<Product, Guid>()
    .AddRepository<Order, int>();
```

Multiple `DbContext` instances are supported — call `AddEntityFrameworkContext` once per context.

#### Entity

```csharp
public class Product : IEntity<Guid>
{
    public Guid Id { get; set; }
    public bool IsDeleted { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
}
```

#### Usage

```csharp
public class ProductService(IRepository<Product, Guid> repo)
{
    public async Task CreateAsync(Product product)
        => await repo.AddAsync(product);

    public async Task<IEnumerable<Product>> SearchAsync(decimal minPrice, int page)
        => await repo.FindAsync(
            filter:     p => p.Price >= minPrice,
            sortOrders: s => s.Ascending(p => p.Name),
            pageIndex:  page,
            pageSize:   20);

    public async Task SetPriceAsync(Guid id, decimal price)
        => await repo.UpdateAsync(id, u => u.Set(p => p.Price, price));

    public async Task RemoveAsync(Guid id)
        => await repo.DeleteAsync(id);              // soft-delete by default
}
```

---

### MongoDB

#### Installation

```
dotnet add package Bellight.MongoDb
```

#### Registration

```csharp
services.AddMongoDb(opts =>
{
    opts.ConnectionString = "mongodb://localhost:27017";
    opts.DatabaseName     = "mydb";
    opts.LogQuery         = false;     // log generated filter definitions
});
```

#### Entity

```csharp
// Base: provides Id (string / ObjectId) and IsDeleted
public class Product : MongoBaseEntity<string>
{
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
}

// Adds CreatedOnUtc, UpdatedOnUtc, CreatedBy, UpdatedBy
public class AuditedProduct : MongoTrackedEntity<string>
{
    public string Name { get; set; } = "";
}
```

Custom collection name:

```csharp
[MongoCollection("products_v2")]
public class Product : MongoBaseEntity<string> { ... }
```

#### Basic usage (via `IRepository<T, string>`)

```csharp
public class ProductService(IRepository<Product, string> repo)
{
    public async Task CreateAsync(Product p)
        => await repo.AddAsync(p);

    public async Task<IEnumerable<Product>> ListExpensiveAsync()
        => await repo.FindAsync(
            filter:     p => p.Price > 100m,
            sortOrders: s => s.Descending(p => p.Price),
            pageIndex:  0,
            pageSize:   50);
}
```

#### Advanced usage (via `IMongoRepository<T>`)

`IMongoRepository<T>` extends `IRepository<T, string>` with direct access to the MongoDB driver builders:

```csharp
public class ProductService(IMongoRepository<Product> repo)
{
    public async Task<IEnumerable<Product>> FindInCategoryAsync(string category)
    {
        var filter = repo.Filter.And(
            repo.FilterBase(),                                 // always include soft-delete guard
            repo.Filter.Eq(p => p.Category, category),
            repo.Filter.Gt(p => p.Price, 0m));

        return await repo.FindAsync(
            filter:    filter,
            sort:      repo.Sort.Descending(p => p.UpdatedOnUtc),
            pageIndex: 0,
            pageSize:  100);
    }

    public async Task IncrementStockAsync(string id, int delta)
        => await repo.UpdateAsync(id,
            repo.Update.Inc(p => p.StockCount, delta));
}
```

**`IMongoRepository<T>` members:**

```csharp
FilterDefinitionBuilder<T>     Filter     { get; }
SortDefinitionBuilder<T>       Sort       { get; }
UpdateDefinitionBuilder<T>     Update     { get; }
ProjectionDefinitionBuilder<T> Project    { get; }
IMongoCollection<T>            Collection { get; }
FilterDefinition<T>            FilterBase();   // pre-built IsDeleted == false guard
```

---

## Message Bus

### Core Concepts

**`IPublisher`** — sends a message to a named topic:

```csharp
void Send(string message);
Task SendAsync(string message, CancellationToken cancellationToken = default);
```

**`IMessageBusFactory`** — resolves publishers and subscriptions:

```csharp
IPublisher     GetPublisher(string topic, MessageBusType type = MessageBusType.Queue);
ISubscription  Subscribe(string topic, Func<string, Task> handler, MessageBusType type = MessageBusType.Queue);
```

**`MessageBusType`** — `Queue` (point-to-point) or `PubSub` (fan-out).

---

### AMQP (RabbitMQ)

#### Installation

```
dotnet add package Bellight.MessageBus.Amqp
```

#### Registration

```csharp
services.AddBellightMessageBus()
    .AddAmqp(opts =>
    {
        opts.Endpoint       = "amqp://guest:guest@localhost:5672";
        opts.InstanceName   = "my-service";   // used as routing prefix
        opts.SubscriberName = "consumer-1";   // unique per subscriber process
    });
```

#### Publishing

```csharp
public class OrderService(IMessageBusFactory bus)
{
    public async Task PlaceOrderAsync(Order order)
    {
        var publisher = bus.GetPublisher("orders", MessageBusType.Queue);
        await publisher.SendAsync(JsonSerializer.Serialize(order));
    }
}
```

#### Subscribing

```csharp
public class OrderProcessor(IMessageBusFactory bus, ILogger<OrderProcessor> logger)
    : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        bus.Subscribe("orders", async message =>
        {
            var order = JsonSerializer.Deserialize<Order>(message);
            await ProcessAsync(order!);
        }, MessageBusType.Queue);

        return Task.CompletedTask;
    }
}
```

#### Configuration reference

| Property | Type | Description |
|---|---|---|
| `Endpoint` | `string` | AMQP broker URL (e.g. `amqp://host:5672`) |
| `InstanceName` | `string` | Service identity used to build address prefixes |
| `SubscriberName` | `string` | Unique name for this consumer process |
| `PollingInterval` | `TimeSpan` | How often to poll for new messages (default: 100 ms) |
| `ReconnectInterval` | `TimeSpan` | Delay before reconnect on failure (default: 5 s) |

---

## Soft Delete Behaviour

All `IRepository` implementations apply an automatic `IsDeleted == false` filter on every read operation (`FindAsync`, `GetByIdAsync`, `CountAsync`, `Exists`). `DeleteAsync` sets `IsDeleted = true` by default; pass `softDelete: false` to perform a hard delete. `ToQueryable()` bypasses the filter and returns all records including deleted ones.

---

## Building

```powershell
# Windows
.\build.ps1

# macOS / Linux
./build.sh
```

Requires .NET 10 SDK.

---

## License

See [LICENSE.txt](LICENSE.txt).
