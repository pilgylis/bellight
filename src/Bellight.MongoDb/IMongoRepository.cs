using System.Linq.Expressions;
using Bellight.DataManagement;
using MongoDB.Driver;

namespace Bellight.MongoDb;

/// <summary>
/// MongoDB-specific repository interface that extends <see cref="IRepository{T, TKey}"/> with
/// direct access to MongoDB driver primitives for advanced query, projection, sort, and update operations.
/// </summary>
/// <typeparam name="T">
/// The entity type. Must implement <see cref="IEntity{TKey}"/> with a <c>string</c> primary key,
/// which exposes an <c>Id</c> property and an <c>IsDeleted</c> flag used for soft-delete operations.
/// </typeparam>
public interface IMongoRepository<T> : IRepository<T, string> where T : class, IEntity<string>
{
    /// <summary>
    /// Returns the base <see cref="FilterDefinition{TDocument}"/> that is pre-composed into every query
    /// executed by this repository. Implementations use this to enforce soft-delete filtering
    /// (e.g., <c>IsDeleted == false</c>) so that deleted records are automatically excluded from results.
    /// </summary>
    /// <returns>A <see cref="FilterDefinition{TDocument}"/> representing the mandatory base conditions.</returns>
    FilterDefinition<T> FilterBase();

    /// <summary>
    /// Gets a <see cref="ProjectionDefinitionBuilder{TDocument}"/> for constructing MongoDB projection definitions.
    /// Use this to specify which fields to include or exclude when querying documents.
    /// </summary>
    ProjectionDefinitionBuilder<T> Project { get; }

    /// <summary>
    /// Gets an <see cref="UpdateDefinitionBuilder{TDocument}"/> for constructing MongoDB update definitions.
    /// Use this to build field-level update operations (e.g., <c>$set</c>, <c>$inc</c>, <c>$push</c>).
    /// </summary>
    UpdateDefinitionBuilder<T> Update { get; }

    /// <summary>
    /// Gets a <see cref="SortDefinitionBuilder{TDocument}"/> for constructing MongoDB sort definitions.
    /// Use this to compose multi-field ascending/descending sort orders.
    /// </summary>
    SortDefinitionBuilder<T> Sort { get; }

    /// <summary>
    /// Gets a <see cref="FilterDefinitionBuilder{TDocument}"/> for constructing MongoDB filter definitions.
    /// Use this to build complex filter expressions to pass to the <c>FindAsync</c>, <c>CountAsync</c>,
    /// <c>Exists</c>, and <c>UpdateManyAsync</c> overloads that accept a <see cref="FilterDefinition{TDocument}"/>.
    /// </summary>
    FilterDefinitionBuilder<T> Filter { get; }

    /// <summary>
    /// Gets the underlying <see cref="IMongoCollection{TDocument}"/> for this repository.
    /// Use this for operations not covered by the repository interface, such as aggregation pipelines
    /// or bulk writes.
    /// </summary>
    IMongoCollection<T> Collection { get; }

    /// <summary>
    /// Returns a paginated list of entities that match the given MongoDB <paramref name="filter"/>.
    /// The base filter from <see cref="FilterBase"/> is automatically combined with <paramref name="filter"/>.
    /// </summary>
    /// <param name="filter">A MongoDB filter definition that identifies which entities to return.</param>
    /// <param name="sort">An optional sort definition. Pass <c>null</c> to use the store's default ordering.</param>
    /// <param name="pageIndex">Zero-based page index. Defaults to <c>0</c>.</param>
    /// <param name="pageSize">Maximum number of results per page. Defaults to <c>20</c>.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>An enumerable of matched entities for the requested page.</returns>
    Task<IEnumerable<T>> FindAsync(
        FilterDefinition<T> filter,
        SortDefinition<T>? sort = null,
        int pageIndex = 0,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a paginated list of projected results for entities that match the given MongoDB <paramref name="filter"/>.
    /// Use this overload to avoid loading full entity graphs when only a subset of fields is needed.
    /// The base filter from <see cref="FilterBase"/> is automatically combined with <paramref name="filter"/>.
    /// </summary>
    /// <typeparam name="P">The projected result type.</typeparam>
    /// <param name="filter">A MongoDB filter definition that identifies which entities to query.</param>
    /// <param name="projection">
    /// A selector expression that maps each matched entity to the projected type <typeparamref name="P"/>.
    /// Example: <c>x => new MyDto { Name = x.Name, Id = x.Id }</c>
    /// </param>
    /// <param name="sort">An optional sort definition. Pass <c>null</c> to use the store's default ordering.</param>
    /// <param name="pageIndex">Zero-based page index. Defaults to <c>0</c>.</param>
    /// <param name="pageSize">Maximum number of results per page. Defaults to <c>20</c>.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>An enumerable of projected values for the requested page.</returns>
    Task<IEnumerable<P>> FindAsync<P>(
        FilterDefinition<T> filter,
        Expression<Func<T, P>> projection,
        SortDefinition<T>? sort = null,
        int pageIndex = 0,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts the number of entities that match the given MongoDB <paramref name="filter"/>.
    /// The base filter from <see cref="FilterBase"/> is automatically combined with <paramref name="filter"/>.
    /// </summary>
    /// <param name="filter">A MongoDB filter definition used to select entities to count.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The count of matching entities.</returns>
    Task<long> CountAsync(FilterDefinition<T> filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether at least one entity matching the given MongoDB <paramref name="filter"/> exists in the collection.
    /// The base filter from <see cref="FilterBase"/> is automatically combined with <paramref name="filter"/>.
    /// </summary>
    /// <param name="filter">A MongoDB filter definition used to test entities.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns><c>true</c> if at least one matching entity exists; otherwise <c>false</c>.</returns>
    Task<bool> Exists(FilterDefinition<T> filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies a MongoDB <paramref name="update"/> operation to all entities that match the given <paramref name="filter"/>.
    /// Only the fields targeted by <paramref name="update"/> are modified; all other fields remain unchanged.
    /// </summary>
    /// <param name="filter">A MongoDB filter definition that identifies which entities to update.</param>
    /// <param name="update">
    /// A MongoDB update definition specifying the field-level changes to apply.
    /// Use the <see cref="Update"/> builder to compose the definition.
    /// Example: <c>repo.Update.Set(x => x.Status, "active").Inc(x => x.RetryCount, 1)</c>
    /// </param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The number of entities that were updated.</returns>
    Task<long> UpdateManyAsync(FilterDefinition<T> filter, UpdateDefinition<T> update, CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies a MongoDB <paramref name="update"/> operation to the entity with the specified <paramref name="id"/>.
    /// Only the fields targeted by <paramref name="update"/> are modified; all other fields remain unchanged.
    /// </summary>
    /// <param name="id">The string primary key of the entity to update.</param>
    /// <param name="update">
    /// A MongoDB update definition specifying the field-level changes to apply.
    /// Use the <see cref="Update"/> builder to compose the definition.
    /// Example: <c>repo.Update.Set(x => x.Name, "new name")</c>
    /// </param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    Task UpdateAsync(string id, UpdateDefinition<T> update, CancellationToken cancellationToken = default);
}