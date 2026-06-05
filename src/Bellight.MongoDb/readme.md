# Bellight.MongoDb

MongoDB repository implementation for the `Bellight.DataManagement` abstraction layer. Provides generic CRUD, soft-delete, pagination, projection, and transaction support built on top of the official MongoDB .NET driver.

---

## Table of Contents

- [Dependencies](#dependencies)
- [Setup](#setup)
- [Configuration](#configuration)
- [Defining Entities](#defining-entities)
- [Registering Repositories](#registering-repositories)
- [Basic Repository Usage](#basic-repository-usage)
- [MongoDB-Native Repository Usage](#mongodb-native-repository-usage)
- [Soft-Delete Behaviour](#soft-delete-behaviour)
- [Transaction Support](#transaction-support)
- [API Reference](#api-reference)

---

## Dependencies

- `MongoDB.Driver`
- `Bellight.DataManagement` (defines `IRepository<T, TKey>`, `IEntity<TKey>`, `IEntityUpdateDefinition`, `IEntitySortDefinition`)
- `Microsoft.Extensions.DependencyInjection.Abstractions`
- `Microsoft.Extensions.Options`
- `Microsoft.Extensions.Logging.Abstractions`

Target framework: `net10.0`

---

## Setup

Register the library in the DI container using `AddMongoDb`:

```csharp
services.AddMongoDb(options =>
{
    options.ConnectionString = "mongodb://localhost:27017";
    options.DatabaseName     = "my_database";
    options.UseSsl           = "false";
    options.DirectConnection = "false";
    options.LogQuery         = "false";
});
```

This registers:
- `ICollectionFactory` → `CollectionFactory` (singleton)
- `MongoDbSettings` bound to the provided configuration

---

## Configuration

`MongoDbSettings` properties:

| Property           | Type     | Default   | Description                                                   |
|--------------------|----------|-----------|---------------------------------------------------------------|
| `ConnectionString` | `string` | —         | MongoDB connection string.                                    |
| `DatabaseName`     | `string` | —         | Name of the MongoDB database to connect to.                   |
| `UseSsl`           | `string` | `"false"` | Set to `"true"` to enable TLS/SSL on the connection.          |
| `DirectConnection` | `string` | `"false"` | Set to `"true"` to force a direct (non-topology-discovery) connection. |
| `LogQuery`         | `string` | —         | Set to `"true"` to emit MongoDB command events to the logger. |

---

## Defining Entities

### Minimal entity — `MongoBaseEntity<TKey>`

Inherit `MongoBaseEntity<TKey>` for the minimum viable entity. It provides `Id` and `IsDeleted`.

```csharp
public class Product : MongoBaseEntity<string>
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
```

`Id` is serialized as a BSON `ObjectId`. `IsDeleted` drives the soft-delete filter applied to every query.

### Tracked entity — `MongoTrackedEntity<TKey>`

Inherit `MongoTrackedEntity<TKey>` to also track audit timestamps and the actor who created/updated the record.

```csharp
[BsonIgnoreExtraElements]  // already applied by MongoTrackedEntity
public class Order : MongoTrackedEntity<string>
{
    public string CustomerId { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
}
```

Additional members provided by `MongoTrackedEntity<TKey>`:

| Member                        | Type        | Description                                          |
|-------------------------------|-------------|------------------------------------------------------|
| `CreatedOnUtc`                | `DateTime?` | Set by `SetCreateBy()`.                              |
| `UpdatedOnUtc`                | `DateTime?` | Set by `SetUpdatedBy()`.                             |
| `CreatedBy`                   | `string?`   | Identifier of the user who created the record.       |
| `UpdatedBy`                   | `string?`   | Identifier of the user who last updated the record.  |
| `SetCreateBy(string actor)`   | `void`      | Assigns `CreatedBy` and stamps `CreatedOnUtc` (UTC). |
| `SetUpdatedBy(string actor)`  | `void`      | Assigns `UpdatedBy` and stamps `UpdatedOnUtc` (UTC). |

### Overriding the collection name — `[MongoCollection]`

By default the collection name is derived from the entity type name. Use `[MongoCollection]` to specify a custom name:

```csharp
[MongoCollection("products_v2")]
public class Product : MongoBaseEntity<string> { }
```

---

## Basic Repository Usage

`MongoRepository<T, TKey>` implements `IRepository<T, TKey>` from `Bellight.DataManagement`. All filter and sort expressions use standard LINQ lambdas.

```csharp
public class ProductService(IRepository<Product, string> repo)
{
    // Insert
    await repo.AddAsync(new Product { Id = ObjectId.GenerateNewId().ToString(), Name = "Widget", Price = 9.99m });

    // Bulk insert
    await repo.AddManyAsync(products);

    // Fetch by ID
    Product? product = await repo.GetByIdAsync(id);

    // Paginated search (page 0, 20 items per page)
    IEnumerable<Product> page = await repo.FindAsync(
        filter:     p => p.Price > 5m,
        sortOrders: s => s.Ascending(p => p.Name),
        pageIndex:  0,
        pageSize:   20);

    // Projected search — fetch only Name and Price
    IEnumerable<ProductDto> dtos = await repo.FindAsync(
        filter:     p => p.Price > 5m,
        projection: p => new ProductDto { Name = p.Name, Price = p.Price });

    // Count
    long count = await repo.CountAsync(p => p.Price > 5m);

    // Existence check
    bool exists = await repo.Exists(p => p.Name == "Widget");

    // Partial update by ID (only the specified fields are written)
    await repo.UpdateAsync(id, u => u.Set(p => p.Price, 12.99m));

    // Bulk partial update
    long updated = await repo.UpdateManyAsync(
        filter: p => p.Price < 1m,
        update: u => u.Set(p => p.Price, 1m));

    // Full document replace
    await repo.ReplaceAsync(id, updatedProduct);

    // Soft delete (sets IsDeleted = true)
    await repo.DeleteAsync(id);

    // Hard delete
    await repo.DeleteAsync(id, softDelete: false);

    // Bulk soft delete
    long deleted = await repo.DeleteManyAsync(p => p.Price < 0m);

    // Bulk delete by IDs
    long deleted = await repo.DeleteManyAsync(ids, softDelete: true);

    // LINQ queryable (includes soft-deleted records)
    IQueryable<Product> all = repo.ToQueryable();
}
```

---

## MongoDB-Native Repository Usage

`ExtendedMongoRepository<T>` (implements `IMongoRepository<T>`) exposes MongoDB driver builder properties directly. Use this when LINQ expressions are insufficient — for example, when you need `$elemMatch`, text search, geospatial queries, or complex `$set`/`$push` update pipelines.

```csharp
public class OrderService(IMongoRepository<Order> repo)
{
    // Build and execute a native filter
    FilterDefinition<Order> filter = repo.Filter.And(
        repo.FilterBase(),                               // always include the soft-delete guard
        repo.Filter.Eq(o => o.Status, OrderStatus.Open),
        repo.Filter.Gt(o => o.CreatedOnUtc, cutoff));

    IEnumerable<Order> orders = await repo.FindAsync(
        filter:    filter,
        sort:      repo.Sort.Descending(o => o.CreatedOnUtc),
        pageIndex: 0,
        pageSize:  50);

    // Projected find — return only the fields needed
    IEnumerable<OrderSummary> summaries = await repo.FindAsync(
        filter:     filter,
        projection: o => new OrderSummary { Id = o.Id, Status = o.Status });

    // Count and existence checks
    long openCount  = await repo.CountAsync(filter);
    bool anyOpen    = await repo.Exists(filter);

    // Single-document update by ID using native UpdateDefinition
    await repo.UpdateAsync(id,
        repo.Update.Set(o => o.Status, OrderStatus.Shipped)
                   .Set(o => o.UpdatedOnUtc, DateTime.UtcNow));

    // Bulk update matching a filter
    long affected = await repo.UpdateManyAsync(
        filter: repo.Filter.Lt(o => o.CreatedOnUtc, expiry),
        update: repo.Update.Set(o => o.Status, OrderStatus.Expired));

    // Direct collection access for aggregation pipelines
    IMongoCollection<Order> col = repo.Collection;
}
```

### Builder properties on `IMongoRepository<T>`

| Property   | Type                                 | Use for                                           |
|------------|--------------------------------------|---------------------------------------------------|
| `Filter`   | `FilterDefinitionBuilder<T>`         | Constructing `FilterDefinition<T>` objects.       |
| `Sort`     | `SortDefinitionBuilder<T>`           | Constructing `SortDefinition<T>` objects.         |
| `Update`   | `UpdateDefinitionBuilder<T>`         | Constructing `UpdateDefinition<T>` objects.       |
| `Project`  | `ProjectionDefinitionBuilder<T>`     | Constructing `ProjectionDefinition<T>` objects.   |
| `Collection` | `IMongoCollection<T>`              | Direct driver access for aggregations, bulk ops.  |

> **Always combine custom filters with `FilterBase()`** when using raw `FilterDefinition<T>` overloads, so soft-deleted records are excluded:
> ```csharp
> var filter = repo.Filter.And(repo.FilterBase(), repo.Filter.Eq(x => x.Status, "active"));
> ```

---

## Soft-Delete Behaviour

- `DeleteAsync` and `DeleteManyAsync` default to `softDelete: true`, which sets `IsDeleted = true` on the matched document(s) rather than removing them.
- All read operations (`FindAsync`, `CountAsync`, `Exists`, `GetByIdAsync`) automatically exclude documents where `IsDeleted == true` by composing `FilterBase()` into the executed query.
- `ToQueryable()` returns the full collection **including** soft-deleted records; apply your own filter when using it.
- Pass `softDelete: false` to any delete method to permanently remove records from the collection.

---

## Transaction Support

> **Important — replica set required.**
> MongoDB multi-document transactions require the server to be running as a replica set.
> This applies even to a single-node deployment: a standalone `mongod` process does **not** support transactions.
> To run a single-node replica set locally, start `mongod` with `--replSet rs0` and initialise it once with `rs.initiate()` in the shell.
> For Docker, add `command: ["--replSet", "rs0"]` to your compose service and run the same init command on first start.

The library integrates with .NET's `System.Transactions` API. Wrap operations in a `TransactionScope` and the MongoDB operations are automatically enlisted in the same session.

```csharp
using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

await repo.AddAsync(order);
await inventoryRepo.UpdateAsync(itemId, u => u.Set(i => i.Stock, newStock));

scope.Complete(); // commits both operations atomically
// Disposing without Complete() triggers a rollback.
```

To enable transaction tracking, wrap the MongoDB client at the driver level using the provided extension methods:

```csharp
IMongoClient client = new MongoClient(connectionString).AsTransactionClient();
IMongoDatabase db   = client.GetDatabase("mydb").AsTransactionDatabase();
IMongoCollection<Order> col = db.GetCollection<Order>("orders").AsTransactionCollection();
```

`CollectionFactory` applies `AsTransactionClient()` automatically when constructing the driver client, so repositories obtained through DI participate in `TransactionScope` without additional wiring.

---

## API Reference

### `MongoRepository<T, TKey>` — full member list

| Member | Signature | Description |
|--------|-----------|-------------|
| `AddAsync` | `Task AddAsync(T, CancellationToken)` | Insert one entity. |
| `AddManyAsync` | `Task AddManyAsync(IEnumerable<T>, CancellationToken)` | Bulk insert. |
| `GetByIdAsync` | `Task<T?> GetByIdAsync(TKey, CancellationToken)` | Fetch by primary key; returns `null` if not found or soft-deleted. |
| `FindAsync` | `Task<IEnumerable<T>> FindAsync(Expression filter, Expression sort, int, int, CancellationToken)` | Paginated search with LINQ filter and sort. |
| `FindAsync<P>` | `Task<IEnumerable<P>> FindAsync<P>(Expression filter, Expression projection, Expression sort, int, int, CancellationToken)` | Paginated search with LINQ projection. |
| `CountAsync` | `Task<long> CountAsync(Expression filter, CancellationToken)` | Count matching entities. |
| `Exists` | `Task<bool> Exists(Expression filter, CancellationToken)` | Return `true` if at least one match exists. |
| `UpdateAsync` | `Task UpdateAsync(TKey, Func<IEntityUpdateDefinition,IEntityUpdateDefinition>, CancellationToken)` | Partial update by ID via fluent builder. |
| `UpdateManyAsync` | `Task<long> UpdateManyAsync(Expression filter, Func<IEntityUpdateDefinition,IEntityUpdateDefinition>, CancellationToken)` | Bulk partial update via fluent builder. |
| `ReplaceAsync` | `Task ReplaceAsync(TKey, T, CancellationToken)` | Full document replacement. |
| `DeleteAsync` | `Task DeleteAsync(TKey, bool softDelete, CancellationToken)` | Delete by ID; soft by default. |
| `DeleteManyAsync` | `Task<long> DeleteManyAsync(Expression filter, bool softDelete, CancellationToken)` | Bulk delete by expression. |
| `DeleteManyAsync` | `Task<long> DeleteManyAsync(IEnumerable<TKey>, bool softDelete, CancellationToken)` | Bulk delete by ID list. |
| `ToQueryable` | `IQueryable<T> ToQueryable()` | Full collection as LINQ queryable (includes soft-deleted). |
| `FilterBase` | `FilterDefinition<T> FilterBase()` | Base soft-delete filter pre-composed into all queries. |
| `Collection` | `IMongoCollection<T>` | Underlying MongoDB collection. |

### Additional members on `IMongoRepository<T>` / `ExtendedMongoRepository<T>`

| Member | Signature | Description |
|--------|-----------|-------------|
| `FindAsync` | `Task<IEnumerable<T>> FindAsync(FilterDefinition<T>, SortDefinition<T>?, int, int, CancellationToken)` | Paginated search with native MongoDB filter. |
| `FindAsync<P>` | `Task<IEnumerable<P>> FindAsync<P>(FilterDefinition<T>, Expression projection, SortDefinition<T>?, int, int, CancellationToken)` | Paginated projected search with native filter. |
| `CountAsync` | `Task<long> CountAsync(FilterDefinition<T>, CancellationToken)` | Count with native filter. |
| `Exists` | `Task<bool> Exists(FilterDefinition<T>, CancellationToken)` | Existence check with native filter. |
| `UpdateAsync` | `Task UpdateAsync(string id, UpdateDefinition<T>, CancellationToken)` | Partial update by ID with native `UpdateDefinition`. |
| `UpdateManyAsync` | `Task<long> UpdateManyAsync(FilterDefinition<T>, UpdateDefinition<T>, CancellationToken)` | Bulk update with native filter and update definitions. |
| `Filter` | `FilterDefinitionBuilder<T>` | MongoDB filter builder. |
| `Sort` | `SortDefinitionBuilder<T>` | MongoDB sort builder. |
| `Update` | `UpdateDefinitionBuilder<T>` | MongoDB update builder. |
| `Project` | `ProjectionDefinitionBuilder<T>` | MongoDB projection builder. |
