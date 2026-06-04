using System.Linq.Expressions;

namespace Bellight.DataManagement;

/// <summary>
/// Generic repository interface providing async CRUD, query, and pagination operations for entities.
/// </summary>
/// <typeparam name="T">
/// The entity type. Must implement <see cref="IEntity{TKey}"/>, which exposes an <c>Id</c> property
/// and an <c>IsDeleted</c> flag used for soft-delete operations.
/// </typeparam>
/// <typeparam name="TKey">
/// The type of the entity's primary key. Must implement <see cref="IEquatable{T}"/> (e.g. <c>string</c>, <c>Guid</c>, <c>int</c>).
/// </typeparam>
/// <remarks>
/// Soft-delete behaviour: by default, delete operations set <c>IsDeleted = true</c> on the entity rather
/// than removing the record from the underlying store. Pass <c>softDelete: false</c> to perform a hard delete.
/// Implementations are expected to filter out soft-deleted records in all read operations unless
/// explicitly queried via <see cref="ToQueryable"/>.
/// </remarks>
public interface IRepository<T, TKey>
    where T : class, IEntity<TKey>
     where TKey: IEquatable<TKey>
{
    /// <summary>
    /// Returns an <see cref="IQueryable{T}"/> over the full entity collection, including soft-deleted records.
    /// Use this for complex LINQ queries that are not covered by the other methods.
    /// </summary>
    /// <returns>An <see cref="IQueryable{T}"/> backed by the underlying data store.</returns>
    IQueryable<T> ToQueryable();

    /// <summary>
    /// Inserts a single entity into the data store.
    /// </summary>
    /// <param name="item">The entity to insert. Its <c>Id</c> must be set before calling this method.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    Task AddAsync(T item, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts multiple entities into the data store in a single operation.
    /// </summary>
    /// <param name="items">The entities to insert. Each entity's <c>Id</c> must be set before calling this method.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    Task AddManyAsync(IEnumerable<T> items, CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies a partial update to the entity with the specified <paramref name="id"/>.
    /// Only the fields set via the <paramref name="update"/> builder are modified; all other fields remain unchanged.
    /// </summary>
    /// <param name="id">The primary key of the entity to update.</param>
    /// <param name="update">
    /// A fluent builder delegate that specifies which fields to change.
    /// Use <see cref="IEntityUpdateDefinition{TObject,TKey}.Set{TField}"/> to assign individual fields.
    /// Example: <c>u => u.Set(x => x.Status, "active").Set(x => x.Count, 5)</c>
    /// </param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    Task UpdateAsync(
        TKey id,
        Func<IEntityUpdateDefinition<T, TKey>, IEntityUpdateDefinition<T, TKey>>update,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies a partial update to all entities that match the given <paramref name="filter"/>.
    /// Only the fields set via the <paramref name="update"/> builder are modified.
    /// </summary>
    /// <param name="filter">A predicate expression that identifies which entities to update.</param>
    /// <param name="update">
    /// A fluent builder delegate that specifies which fields to change.
    /// Example: <c>u => u.Set(x => x.IsActive, false)</c>
    /// </param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The number of entities that were updated.</returns>
    Task<long> UpdateManyAsync(
        Expression<Func<T, bool>> filter,
        Func<IEntityUpdateDefinition<T, TKey>, IEntityUpdateDefinition<T, TKey>> update,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces the entire entity identified by <paramref name="id"/> with <paramref name="item"/>.
    /// Unlike <see cref="UpdateAsync"/>, this overwrites all fields of the stored document.
    /// </summary>
    /// <param name="id">The primary key of the entity to replace.</param>
    /// <param name="item">The new entity value. Its <c>Id</c> should match <paramref name="id"/>.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    Task ReplaceAsync(TKey id, T item, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the entity with the specified <paramref name="id"/>.
    /// By default this is a soft delete: it sets <c>IsDeleted = true</c> on the entity.
    /// </summary>
    /// <param name="id">The primary key of the entity to delete.</param>
    /// <param name="softDelete">
    /// <c>true</c> (default) to set <c>IsDeleted = true</c>; <c>false</c> to permanently remove the record.
    /// </param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    Task DeleteAsync(TKey id, bool softDelete = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all entities that match the given <paramref name="filter"/>.
    /// By default this is a soft delete: it sets <c>IsDeleted = true</c> on each matched entity.
    /// </summary>
    /// <param name="filter">A predicate expression that identifies which entities to delete.</param>
    /// <param name="softDelete">
    /// <c>true</c> (default) to set <c>IsDeleted = true</c>; <c>false</c> to permanently remove records.
    /// </param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The number of entities that were deleted.</returns>
    Task<long> DeleteManyAsync(
        Expression<Func<T, bool>> filter,
        bool softDelete = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all entities whose primary keys are contained in <paramref name="ids"/>.
    /// </summary>
    /// <param name="ids">The collection of primary keys to delete.</param>
    /// <param name="softDelete">
    /// <c>true</c> to set <c>IsDeleted = true</c> on each entity; <c>false</c> to permanently remove records.
    /// </param>
    /// <param name="token">Token to cancel the asynchronous operation.</param>
    /// <returns>The number of entities that were deleted.</returns>
    Task<long> DeleteManyAsync(IEnumerable<TKey> ids, bool softDelete, CancellationToken token = default);

    /// <summary>
    /// Retrieves a single entity by its primary key.
    /// </summary>
    /// <param name="id">The primary key to look up.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The matching entity, or <c>null</c> if no entity with the given <paramref name="id"/> exists.</returns>
    Task<T?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a paginated list of entities that match the given <paramref name="filter"/>.
    /// </summary>
    /// <param name="filter">A predicate expression used to filter entities.</param>
    /// <param name="sortOrders">
    /// An optional fluent sort builder. Use <see cref="IEntitySortDefinition{TObject,TKey}.Ascending"/> and
    /// <see cref="IEntitySortDefinition{TObject,TKey}.Descending"/> to compose multi-field ordering.
    /// Example: <c>s => s.Ascending(x => x.Name).Descending(x => x.CreatedAt)</c>
    /// Pass <c>null</c> to use the store's default ordering.
    /// </param>
    /// <param name="pageIndex">Zero-based page index. Defaults to <c>0</c>.</param>
    /// <param name="pageSize">Maximum number of results per page. Defaults to <c>20</c>.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>An enumerable of matched entities for the requested page.</returns>
    Task<IEnumerable<T>> FindAsync(
        Expression<Func<T, bool>> filter,
        Expression<Func<IEntitySortDefinition<T, TKey>, IEntitySortDefinition<T, TKey>>>? sortOrders = null,
        int pageIndex = 0,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a paginated list of projected results for entities that match the given <paramref name="filter"/>.
    /// Use this overload to avoid loading full entity graphs when only a subset of fields is needed.
    /// </summary>
    /// <typeparam name="P">The projected result type.</typeparam>
    /// <param name="filter">A predicate expression used to filter entities.</param>
    /// <param name="projection">
    /// A selector expression that maps each matched entity to the projected type <typeparamref name="P"/>.
    /// Example: <c>x => new MyDto { Name = x.Name, Id = x.Id }</c>
    /// </param>
    /// <param name="sortOrders">
    /// An optional fluent sort builder. See the non-projected overload for usage details.
    /// </param>
    /// <param name="pageIndex">Zero-based page index. Defaults to <c>0</c>.</param>
    /// <param name="pageSize">Maximum number of results per page. Defaults to <c>20</c>.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>An enumerable of projected values for the requested page.</returns>
    Task<IEnumerable<P>> FindAsync<P>(
        Expression<Func<T, bool>> filter,
        Expression<Func<T, P>> projection,
        Expression<Func<IEntitySortDefinition<T, TKey>, IEntitySortDefinition<T, TKey>>>? sortOrders = null,
        int pageIndex = 0,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts the number of entities that match the given <paramref name="filter"/>.
    /// </summary>
    /// <param name="filter">A predicate expression used to filter entities.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The count of matching entities.</returns>
    Task<long> CountAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether at least one entity matching the given <paramref name="filter"/> exists in the store.
    /// </summary>
    /// <param name="filter">A predicate expression used to test entities.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns><c>true</c> if at least one matching entity exists; otherwise <c>false</c>.</returns>
    Task<bool> Exists(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default);
}
