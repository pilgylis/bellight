# Bellight.EntityFrameworkCore

Entity Framework Core implementation of the `Bellight.DataManagement` repository abstraction. Provides generic CRUD, soft-delete, pagination, projection, and partial-update support on top of any EF Core-compatible relational database.

---

## Table of Contents

- [Dependencies](#dependencies)
- [Setup](#setup)
- [Defining Entities](#defining-entities)
- [Registering Repositories](#registering-repositories)
- [Usage](#usage)
- [Partial Updates](#partial-updates)
- [Sorting](#sorting)
- [Soft-Delete Behaviour](#soft-delete-behaviour)
- [Multiple DbContexts](#multiple-dbcontexts)
- [API Reference](#api-reference)
- [Limitations](#limitations)

---

## Dependencies

- `Microsoft.EntityFrameworkCore.Relational`
- `Bellight.DataManagement` (defines `IRepository<T, TKey>`, `IEntity<TKey>`, `IEntityUpdateDefinition`, `IEntitySortDefinition`)
- `Microsoft.Extensions.DependencyInjection`

Target framework: `net10.0`

---

## Setup

Install the package:

```bash
dotnet add package Bellight.EntityFrameworkCore
```

Then register a context and its repositories in one fluent call:

```csharp
services.AddEntityFrameworkContext<AppDbContext>(opts =>
        opts.UseSqlServer(configuration.GetConnectionString("Default")))
    .AddRepository<Product, Guid>()
    .AddRepository<Order, string>();
```

`AddEntityFrameworkContext` calls `services.AddDbContext<TDbContext>` internally. If you have already registered the `DbContext` yourself, omit the configure action:

```csharp
// DbContext already registered elsewhere
services.AddEntityFrameworkContext<AppDbContext>()
    .AddRepository<Product, Guid>()
    .AddRepository<Order, string>();
```

---

## Defining Entities

All entities must implement `IEntity<TKey>` from `Bellight.DataManagement`:

```csharp
public interface IEntity<TKey> where TKey : IEquatable<TKey>
{
    TKey Id { get; set; }
    bool IsDeleted { get; set; }
}
```

A typical entity:

```csharp
public class Product : IEntity<Guid>
{
    public Guid Id { get; set; }
    public bool IsDeleted { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
```

Configure it in your `DbContext` as normal:

```csharp
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Product>().HasKey(p => p.Id);
        builder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted); // optional global filter
    }
}
```

> **Note:** If you add a global query filter for `IsDeleted` on the `DbContext`, EF Core and `EntityFrameworkRepository` will both exclude soft-deleted records. Without the global filter, `FindAsync` still excludes them because the repository composes the `IsDeleted == false` predicate into every query automatically.

---

## Registering Repositories

Each `AddRepository<TEntity, TKey>()` call registers a scoped `IRepository<TEntity, TKey>` backed by the specified `DbContext`. Inject it using the standard `IRepository<T, TKey>` interface:

```csharp
public class ProductService(IRepository<Product, Guid> repository)
{
    // ...
}
```

---

## Usage

```csharp
public class ProductService(IRepository<Product, Guid> repo)
{
    // Insert one
    await repo.AddAsync(new Product { Id = Guid.NewGuid(), Name = "Widget", Price = 9.99m });

    // Bulk insert
    await repo.AddManyAsync(products);

    // Fetch by ID (returns null if not found or soft-deleted)
    Product? product = await repo.GetByIdAsync(id);

    // Paginated search
    IEnumerable<Product> page = await repo.FindAsync(
        filter:     p => p.Price > 5m,
        sortOrders: s => s.Ascending(p => p.Name),
        pageIndex:  0,
        pageSize:   20);

    // Projected search — fetch only the fields you need
    IEnumerable<ProductSummary> summaries = await repo.FindAsync(
        filter:     p => p.CategoryId == categoryId,
        projection: p => new ProductSummary { Id = p.Id, Name = p.Name, Price = p.Price },
        sortOrders: s => s.Descending(p => p.Price),
        pageIndex:  0,
        pageSize:   50);

    // Count
    long count = await repo.CountAsync(p => p.Price > 5m);

    // Existence check
    bool exists = await repo.Exists(p => p.Name == "Widget");

    // Partial update by ID
    await repo.UpdateAsync(id, u => u
        .Set(p => p.Price, 12.99m)
        .Set(p => p.UpdatedAt, DateTime.UtcNow));

    // Bulk partial update
    long updated = await repo.UpdateManyAsync(
        filter: p => p.Price < 1m,
        update: u => u.Set(p => p.Price, 1m));

    // Soft delete (sets IsDeleted = true)
    await repo.DeleteAsync(id);

    // Hard delete
    await repo.DeleteAsync(id, softDelete: false);

    // Bulk soft delete by filter
    long deleted = await repo.DeleteManyAsync(p => p.ExpiresAt < DateTime.UtcNow);

    // Bulk delete by ID list
    long deleted = await repo.DeleteManyAsync(ids, softDelete: false);

    // Raw IQueryable — includes soft-deleted records
    IQueryable<Product> all = repo.ToQueryable();
}
```

---

## Partial Updates

`UpdateAsync` and `UpdateManyAsync` use `IEntityUpdateDefinition<T, TKey>` to build field-level updates. Only the fields you specify are written; no full-entity load is required.

Internally this translates to EF Core's `ExecuteUpdateAsync` with `SetProperty` calls, so it issues a single `UPDATE` statement without loading the entity into the change tracker.

```csharp
await repo.UpdateAsync(id, u => u
    .Set(p => p.Status, ProductStatus.Archived)
    .Set(p => p.ArchivedAt, DateTime.UtcNow));
```

---

## Sorting

`FindAsync` accepts an optional `sortOrders` expression built with `IEntitySortDefinition<T, TKey>`:

```csharp
await repo.FindAsync(
    filter:     p => p.IsActive,
    sortOrders: s => s.Ascending(p => p.Category).Descending(p => p.Price));
```

Multiple sort fields are applied in the order they are chained.

---

## Soft-Delete Behaviour

- `DeleteAsync` and `DeleteManyAsync` default to `softDelete: true`, which sets `IsDeleted = true` on matched rows using `ExecuteUpdateAsync` — no entity tracking involved.
- All read operations (`FindAsync`, `CountAsync`, `Exists`, `GetByIdAsync`) automatically exclude rows where `IsDeleted == true`.
- `ToQueryable()` returns the full `IQueryable<T>` **including** soft-deleted records; apply your own filter when using it directly.
- Pass `softDelete: false` to any delete method for a permanent `DELETE` via `ExecuteDeleteAsync`.

---

## Multiple DbContexts

Call `AddEntityFrameworkContext` once per context. Each `AddRepository` call registers its `IRepository<TEntity, TKey>` as a factory that resolves its own specific `DbContext`, so there is no cross-context interference:

```csharp
services
    .AddEntityFrameworkContext<AppDbContext>(opts => opts.UseSqlServer(appConnStr))
    .AddRepository<Product, Guid>()
    .AddRepository<Order, string>();

services
    .AddEntityFrameworkContext<AnalyticsDbContext>(opts => opts.UseNpgsql(analyticsConnStr))
    .AddRepository<Report, int>()
    .AddRepository<Event, long>();
```

---

## API Reference

### `EntityFrameworkRepository<TObject, TKey>`

| Member | Signature | Description |
|--------|-----------|-------------|
| `AddAsync` | `Task AddAsync(T, CancellationToken)` | Insert one entity and save. |
| `AddManyAsync` | `Task AddManyAsync(IEnumerable<T>, CancellationToken)` | Bulk insert via `AddRangeAsync`. |
| `GetByIdAsync` | `Task<T?> GetByIdAsync(TKey, CancellationToken)` | Fetch by primary key; returns `null` if not found or soft-deleted. |
| `FindAsync` | `Task<IEnumerable<T>> FindAsync(filter, sortOrders?, pageIndex, pageSize, CancellationToken)` | Paginated, sorted query with LINQ filter. |
| `FindAsync<P>` | `Task<IEnumerable<P>> FindAsync<P>(filter, projection, sortOrders?, pageIndex, pageSize, CancellationToken)` | Same as above with a projection to type `P`. |
| `CountAsync` | `Task<long> CountAsync(filter, CancellationToken)` | Count matching non-deleted entities. |
| `Exists` | `Task<bool> Exists(filter, CancellationToken)` | Returns `true` if at least one match exists. |
| `UpdateAsync` | `Task UpdateAsync(TKey, Func<IEntityUpdateDefinition, IEntityUpdateDefinition>, CancellationToken)` | Partial update by ID via fluent builder — single `UPDATE` statement. |
| `UpdateManyAsync` | `Task<long> UpdateManyAsync(filter, Func<IEntityUpdateDefinition, IEntityUpdateDefinition>, CancellationToken)` | Bulk partial update. Returns rows affected. |
| `DeleteAsync` | `Task DeleteAsync(TKey, bool softDelete, CancellationToken)` | Delete by ID; soft by default. |
| `DeleteManyAsync` | `Task<long> DeleteManyAsync(filter, bool softDelete, CancellationToken)` | Bulk delete by filter. Returns rows affected. |
| `DeleteManyAsync` | `Task<long> DeleteManyAsync(IEnumerable<TKey>, bool softDelete, CancellationToken)` | Bulk delete by ID list. Returns rows affected. |
| `ToQueryable` | `IQueryable<T> ToQueryable()` | Full `DbSet<T>` as `IQueryable` — includes soft-deleted records. |

### `ServiceCollectionExtensions`

| Method | Description |
|--------|-------------|
| `AddEntityFrameworkContext<TDbContext>(configure?)` | Optionally registers `TDbContext` via `AddDbContext`, then returns an `EntityFrameworkBuilder<TDbContext>` for chaining repository registrations. |

### `EntityFrameworkBuilder<TDbContext>`

| Method | Description |
|--------|-------------|
| `AddRepository<TEntity, TKey>()` | Registers a scoped `IRepository<TEntity, TKey>` backed by `TDbContext`. Returns `this` for chaining. |

---

## Limitations

- `ReplaceAsync` is not implemented and throws `NotImplementedException`. EF Core's change-tracking model does not map cleanly to a full-document replace; use `UpdateAsync` with explicit field assignments instead.

---

## License

Copyright Nguyen Viet Trung. See [LICENSE](../../LICENSE.txt).
