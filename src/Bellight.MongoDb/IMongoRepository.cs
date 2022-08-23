using Bellight.DataManagement;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Bellight.MongoDb
{
    public interface IMongoRepository<T, Tid>: IRepository<T, Tid> where T : class, IEntity<Tid>
    {
        IMongoCollection<T> Collection { get; }

        Task UpdateAsync(Tid id, UpdateDefinition<T> update, CancellationToken cancellationToken = default);
        Task UpdateAsync(Tid id, Func<UpdateDefinition<T>, UpdateDefinition<T>> updateFunc, CancellationToken cancellationToken = default);
        Task<long> UpdateManyAsync(
            Expression<Func<T, bool>> filter,
            UpdateDefinition<T> update,
            CancellationToken cancellationToken = default);
        Task<long> UpdateManyAsync(
            Expression<Func<T, bool>> filter,
            Func<UpdateDefinition<T>, UpdateDefinition<T>> updateFunc,
            CancellationToken cancellationToken = default);

        Task<long> UpdateManyAsync(FilterDefinition<T> filter, UpdateDefinition<T> update, CancellationToken cancellationToken = default);
        Task<long> UpdateManyAsync(FilterDefinition<T> filter, Func<UpdateDefinition<T>, UpdateDefinition<T>> updateFunc, CancellationToken cancellationToken = default);
        FilterDefinitionBuilder<T> Filter { get; }
        SortDefinitionBuilder<T> Sort { get; }
        UpdateDefinitionBuilder<T> Update { get; }
        ProjectionDefinitionBuilder<T> Project { get; }
        FilterDefinition<T> FilterBase();

        Task<IEnumerable<T>> FindAsync(FilterDefinition<T> filter, SortDefinition<T>? sort = null, int pageIndex = 0, int pageSize = 20, CancellationToken cancellationToken = default);

        Task<IEnumerable<P>> FindAsync<P>(FilterDefinition<T> filter, Expression<Func<T, P>> projection, SortDefinition<T>? sort = null, int pageIndex = 0, int pageSize = 20, CancellationToken cancellationToken = default);

        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> filter, SortDefinition<T>? sort = null, int pageIndex = 0, int pageSize = 20, CancellationToken cancellationToken = default);
        Task<long> CountAsync(FilterDefinition<T> filter, CancellationToken cancellationToken = default);
        Task<bool> Exists(FilterDefinition<T> filter, CancellationToken cancellationToken = default);

        SortDefinition<T> CreateSortDefinition(IEnumerable<KeyValuePair<string, bool>>? sortOrders);
    }
}
