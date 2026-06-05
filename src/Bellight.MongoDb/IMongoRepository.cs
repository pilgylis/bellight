using System.Linq.Expressions;
using Bellight.DataManagement;
using MongoDB.Driver;

namespace Bellight.MongoDb;
public interface IMongoRepository<T>: IRepository<T, string> where T : class, IEntity<string>
{
    FilterDefinition<T> FilterBase();
    ProjectionDefinitionBuilder<T> Project { get; }
    UpdateDefinitionBuilder<T> Update { get; }
    SortDefinitionBuilder<T> Sort { get; }
    FilterDefinitionBuilder<T> Filter { get; }
    IMongoCollection<T> Collection { get; }

    Task<IEnumerable<T>> FindAsync(
        FilterDefinition<T> filter,
        SortDefinition<T>? sort = null,
        int pageIndex = 0,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<P>> FindAsync<P>(
        FilterDefinition<T> filter,
        Expression<Func<T, P>> projection,
        SortDefinition<T>? sort = null,
        int pageIndex = 0,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    Task<long> CountAsync(FilterDefinition<T> filter, CancellationToken cancellationToken = default);
    Task<bool> Exists(FilterDefinition<T> filter, CancellationToken cancellationToken = default);

    Task<long> UpdateManyAsync(FilterDefinition<T> filter, UpdateDefinition<T> update, CancellationToken cancellationToken = default);
    Task UpdateAsync(string id, UpdateDefinition<T> update, CancellationToken cancellationToken = default);
}