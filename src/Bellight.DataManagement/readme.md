# Bellight.DataManagement

Data management abstraction for the Bellight framework. Provides a generic, async repository interface (`IRepository<T, TKey>`) that decouples your application from any specific data store.

Concrete implementations are available as separate packages:

| Package | Backing store |
|---|---|
| `Bellight.MongoDb` | MongoDB |
| `Bellight.EntityFrameworkCore` | Entity Framework Core (relational) |

## Getting Started

Install the abstraction package:

```bash
dotnet add package Bellight.DataManagement
```

Then install one of the implementation packages and register it with DI.

## Core Concepts

### IEntity\<TKey\>

All entities must implement `IEntity<TKey>`:

```csharp
public interface IEntity<IdType> where IdType : IEquatable<IdType>
{
    IdType Id { get; set; }
    bool IsDeleted { get; set; }
}
```

### Soft Delete

Delete operations set `IsDeleted = true` by default. All read operations exclude soft-deleted records unless you bypass them via `ToQueryable()`. Pass `softDelete: false` to permanently remove a record.

### IRepository\<T, TKey\>

The main abstraction. `T` is your entity type; `TKey` is the primary-key type (`string`, `Guid`, `int`, etc.).

```csharp
public interface IRepository<T, TKey>
    where T : class, IEntity<TKey>
    where TKey : IEquatable<TKey>
```

## API Reference

### Write

| Method | Description |
|---|---|
| `AddAsync(item)` | Insert a single entity. |
| `AddManyAsync(items)` | Bulk-insert multiple entities. |
| `UpdateAsync(id, update)` | Partial update by primary key — only specified fields are changed. |
| `UpdateManyAsync(filter, update)` | Partial update on all matched entities. Returns count updated. |
| `ReplaceAsync(id, item)` | Full document replace — all fields are overwritten. |
| `DeleteAsync(id, softDelete)` | Delete by primary key. Soft by default. |
| `DeleteManyAsync(filter, softDelete)` | Delete all matched entities. Returns count deleted. |
| `DeleteManyAsync(ids, softDelete)` | Delete by a list of primary keys. Returns count deleted. |

### Read

| Method | Description |
|---|---|
| `GetByIdAsync(id)` | Fetch a single entity by primary key. Returns `null` if not found or soft-deleted. |
| `FindAsync(filter, sortOrders, pageIndex, pageSize)` | Paginated, sorted query. Excludes soft-deleted records. |
| `FindAsync<P>(filter, projection, sortOrders, pageIndex, pageSize)` | Same as above, with a projection to type `P`. |
| `CountAsync(filter)` | Count matching non-deleted entities. |
| `Exists(filter)` | Returns `true` if at least one matching non-deleted entity exists. |
| `ToQueryable()` | Raw `IQueryable<T>` including soft-deleted records — use for complex queries only. |

## Partial Updates

Use `IEntityUpdateDefinition<T, TKey>` to fluently compose field-level updates:

```csharp
await repository.UpdateAsync(id, u => u
    .Set(x => x.Status, "active")
    .Set(x => x.UpdatedAt, DateTime.UtcNow));
```

## Sorting

Use `IEntitySortDefinition<T, TKey>` to compose multi-field ordering:

```csharp
await repository.FindAsync(
    filter: x => x.IsActive,
    sortOrders: s => s.Ascending(x => x.LastName).Descending(x => x.CreatedAt));
```

## Examples

### Paginated query with projection

```csharp
var dtos = await repository.FindAsync<ProductSummary>(
    filter: x => x.CategoryId == id,
    projection: x => new ProductSummary { Id = x.Id, Name = x.Name, Price = x.Price },
    sortOrders: s => s.Ascending(x => x.Name),
    pageIndex: 0,
    pageSize: 50);
```

### Check-then-insert

```csharp
if (!await repository.Exists(x => x.Sku == sku))
    await repository.AddAsync(new Product { Id = Guid.NewGuid(), Sku = sku });
```

### Bulk soft-delete

```csharp
long deleted = await repository.DeleteManyAsync(x => x.ExpiresAt < DateTime.UtcNow);
```

### Pagination loop

```csharp
int page = 0;
IEnumerable<Product> batch;
do
{
    batch = await repository.FindAsync(x => x.IsActive, pageIndex: page++, pageSize: 100);
    foreach (var item in batch) { /* process */ }
} while (batch.Any());
```

## License

Copyright Nguyen Viet Trung. See [LICENSE](../../LICENSE.txt).
