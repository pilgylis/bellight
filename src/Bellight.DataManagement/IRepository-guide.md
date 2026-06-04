# IRepository\<T, TKey\> — Developer Guide

## Overview

`IRepository<T, TKey>` is a generic async repository abstraction in the `Bellight.DataManagement` namespace.
It provides a uniform API for CRUD, bulk operations, filtering, sorting, and pagination against any backing
store (e.g. MongoDB via `Bellight.MongoDb`).

---

## Type Constraints

| Type param | Constraint | Meaning |
|---|---|---|
| `T` | `class, IEntity<TKey>` | The entity type. Must have an `Id` and an `IsDeleted` flag. |
| `TKey` | `IEquatable<TKey>` | The primary-key type — typically `string`, `Guid`, or `int`. |

### IEntity\<TKey\>

```csharp
public interface IEntity<IdType> where IdType : IEquatable<IdType>
{
    IdType Id { get; set; }
    bool IsDeleted { get; set; }
}
```

All entities carry an `IsDeleted` flag. **Read operations return only non-deleted records** unless you
bypass them via `ToQueryable()`. Delete operations set this flag by default (soft delete).

---

## Supporting Types

### IEntityUpdateDefinition\<TObject, TKey\>

Fluent builder for partial updates. Chain `.Set()` calls to specify only the fields that should change.

```csharp
public interface IEntityUpdateDefinition<TObject, TKey>
{
    IEntityUpdateDefinition<TObject, TKey> Set<TField>(
        Expression<Func<TObject, TField>> field,
        TField fieldValue);
}
```

**Example:**

```csharp
u => u.Set(x => x.Status, "active")
      .Set(x => x.UpdatedAt, DateTime.UtcNow)
```

### IEntitySortDefinition\<TObject, TKey\>

Fluent builder for multi-field ordering. Chain `.Ascending()` / `.Descending()` calls.

```csharp
public interface IEntitySortDefinition<TObject, TKey>
{
    IEntitySortDefinition<TObject, TKey> Ascending(Expression<Func<TObject, object>> field);
    IEntitySortDefinition<TObject, TKey> Descending(Expression<Func<TObject, object>> field);
}
```

**Example:**

```csharp
s => s.Ascending(x => x.LastName)
      .Descending(x => x.CreatedAt)
```

---

## Method Reference

### ToQueryable

```csharp
IQueryable<T> ToQueryable();
```

Returns a raw `IQueryable<T>` over the full collection, **including soft-deleted records**.
Use only when the other methods are insufficient (e.g. complex joins or projections).

---

### AddAsync

```csharp
Task AddAsync(T item, CancellationToken cancellationToken = default);
```

Inserts a single entity. The entity's `Id` must be assigned before calling.

**Example:**

```csharp
var product = new Product { Id = Guid.NewGuid(), Name = "Widget" };
await repository.AddAsync(product);
```

---

### AddManyAsync

```csharp
Task AddManyAsync(IEnumerable<T> items, CancellationToken cancellationToken = default);
```

Bulk-inserts multiple entities in a single operation.

**Example:**

```csharp
var products = items.Select(i => new Product { Id = Guid.NewGuid(), Name = i.Name });
await repository.AddManyAsync(products);
```

---

### UpdateAsync

```csharp
Task UpdateAsync(
    TKey id,
    Func<IEntityUpdateDefinition<T, TKey>, IEntityUpdateDefinition<T, TKey>> update,
    CancellationToken cancellationToken = default);
```

Partially updates a single entity by primary key. Only the fields specified via the builder are written;
all other fields are left unchanged.

**Example:**

```csharp
await repository.UpdateAsync(id, u => u.Set(x => x.Stock, 42));
```

---

### UpdateManyAsync

```csharp
Task<long> UpdateManyAsync(
    Expression<Func<T, bool>> filter,
    Func<IEntityUpdateDefinition<T, TKey>, IEntityUpdateDefinition<T, TKey>> update,
    CancellationToken cancellationToken = default);
```

Partially updates all entities matching `filter`. Returns the number of entities updated.

**Example:**

```csharp
long count = await repository.UpdateManyAsync(
    x => x.CategoryId == categoryId,
    u => u.Set(x => x.IsActive, false));
```

---

### ReplaceAsync

```csharp
Task ReplaceAsync(TKey id, T item, CancellationToken cancellationToken = default);
```

Replaces the **entire** stored document for the entity with the given `id`.
Unlike `UpdateAsync`, every field is overwritten. The `item.Id` should equal `id`.

**Example:**

```csharp
product.Name = "Updated Widget";
await repository.ReplaceAsync(product.Id, product);
```

---

### DeleteAsync

```csharp
Task DeleteAsync(TKey id, bool softDelete = true, CancellationToken cancellationToken = default);
```

Deletes a single entity by primary key.

| `softDelete` | Behaviour |
|---|---|
| `true` (default) | Sets `IsDeleted = true`. The record remains in the store. |
| `false` | Permanently removes the record. |

**Example:**

```csharp
await repository.DeleteAsync(id);               // soft delete
await repository.DeleteAsync(id, softDelete: false); // hard delete
```

---

### DeleteManyAsync (by filter)

```csharp
Task<long> DeleteManyAsync(
    Expression<Func<T, bool>> filter,
    bool softDelete = true,
    CancellationToken cancellationToken = default);
```

Deletes all entities matching `filter`. Returns the number of entities deleted.

**Example:**

```csharp
long removed = await repository.DeleteManyAsync(x => x.ExpiresAt < DateTime.UtcNow);
```

---

### DeleteManyAsync (by IDs)

```csharp
Task<long> DeleteManyAsync(IEnumerable<TKey> ids, bool softDelete, CancellationToken token = default);
```

Deletes entities whose primary keys are in `ids`. Note: `softDelete` has **no default** on this overload
and must be supplied explicitly.

**Example:**

```csharp
long removed = await repository.DeleteManyAsync(expiredIds, softDelete: true);
```

---

### GetByIdAsync

```csharp
Task<T?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);
```

Fetches a single entity by primary key. Returns `null` if not found or if the entity is soft-deleted.

**Example:**

```csharp
var product = await repository.GetByIdAsync(id);
if (product is null) { /* not found */ }
```

---

### FindAsync (entities)

```csharp
Task<IEnumerable<T>> FindAsync(
    Expression<Func<T, bool>> filter,
    Expression<Func<IEntitySortDefinition<T, TKey>, IEntitySortDefinition<T, TKey>>>? sortOrders = null,
    int pageIndex = 0,
    int pageSize = 20,
    CancellationToken cancellationToken = default);
```

Returns a paginated, sorted list of entities matching `filter`. Soft-deleted records are excluded.

| Parameter | Default | Notes |
|---|---|---|
| `filter` | — | Required. Use `x => true` to match all. |
| `sortOrders` | `null` | Optional. `null` uses the store's default order. |
| `pageIndex` | `0` | Zero-based. |
| `pageSize` | `20` | Max records per page. |

**Example:**

```csharp
var page = await repository.FindAsync(
    filter: x => x.CategoryId == id && x.IsActive,
    sortOrders: s => s.Ascending(x => x.Name),
    pageIndex: 2,
    pageSize: 50);
```

---

### FindAsync\<P\> (with projection)

```csharp
Task<IEnumerable<P>> FindAsync<P>(
    Expression<Func<T, bool>> filter,
    Expression<Func<T, P>> projection,
    Expression<Func<IEntitySortDefinition<T, TKey>, IEntitySortDefinition<T, TKey>>>? sortOrders = null,
    int pageIndex = 0,
    int pageSize = 20,
    CancellationToken cancellationToken = default);
```

Same as `FindAsync` but maps each result to a projected type `P`. Use this to avoid loading full entity
graphs when only a subset of fields is needed.

**Example:**

```csharp
var names = await repository.FindAsync<string>(
    filter: x => x.IsActive,
    projection: x => x.Name,
    sortOrders: s => s.Ascending(x => x.Name));
```

**DTO projection example:**

```csharp
var dtos = await repository.FindAsync<ProductSummary>(
    filter: x => x.CategoryId == id,
    projection: x => new ProductSummary { Id = x.Id, Name = x.Name, Price = x.Price });
```

---

### CountAsync

```csharp
Task<long> CountAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default);
```

Returns the count of non-deleted entities matching `filter`.

**Example:**

```csharp
long activeCount = await repository.CountAsync(x => x.IsActive);
```

---

### Exists

```csharp
Task<bool> Exists(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default);
```

Returns `true` if at least one non-deleted entity matches `filter`. More efficient than `CountAsync > 0`
when you only need existence.

**Example:**

```csharp
bool taken = await repository.Exists(x => x.Email == email);
```

---

## Common Patterns

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

### Check-then-insert

```csharp
if (!await repository.Exists(x => x.Sku == sku))
    await repository.AddAsync(new Product { Id = Guid.NewGuid(), Sku = sku });
```

### Soft-delete all expired records

```csharp
long deleted = await repository.DeleteManyAsync(x => x.ExpiresAt < DateTime.UtcNow);
```

### Bulk partial update

```csharp
await repository.UpdateManyAsync(
    x => x.TenantId == tenantId,
    u => u.Set(x => x.Plan, "enterprise").Set(x => x.UpdatedAt, DateTime.UtcNow));
```
