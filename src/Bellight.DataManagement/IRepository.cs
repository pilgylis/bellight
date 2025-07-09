using System.Linq.Expressions;

namespace Bellight.DataManagement;

public interface IRepository<T, TKey> 
    where T : class, IEntity<TKey>
     where TKey: IEquatable<TKey>
{
    IQueryable<T> ToQueryable();

    Task AddAsync(T item, CancellationToken cancellationToken = default);

    Task AddManyAsync(IEnumerable<T> items, CancellationToken cancellationToken = default);

    Task UpdateAsync(
        TKey id,
        Func<IEntityUpdateDefinition<T, TKey>, IEntityUpdateDefinition<T, TKey>>update,
        CancellationToken cancellationToken = default);

    Task<long> UpdateManyAsync(
        Expression<Func<T, bool>> filter,
        Func<IEntityUpdateDefinition<T, TKey>, IEntityUpdateDefinition<T, TKey>> update,
        CancellationToken cancellationToken = default);

    Task ReplaceAsync(TKey id, T item, CancellationToken cancellationToken = default);

    Task DeleteAsync(TKey id, bool softDelete = true, CancellationToken cancellationToken = default);

    Task<long> DeleteManyAsync(
        Expression<Func<T, bool>> filter,
        bool softDelete = true,
        CancellationToken cancellationToken = default);

    Task<long> DeleteManyAsync(IEnumerable<TKey> ids, bool softDelete, CancellationToken token = default);

    Task<T?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> FindAsync(
        Expression<Func<T, bool>> filter,
        Expression<Func<IEntitySortDefinition<T, TKey>, IEntitySortDefinition<T, TKey>>>? sortOrders = null,
        int pageIndex = 0,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<P>> FindAsync<P>(
        Expression<Func<T, bool>> filter,
        Expression<Func<T, P>> projection,
        Expression<Func<IEntitySortDefinition<T, TKey>, IEntitySortDefinition<T, TKey>>>? sortOrders = null,
        int pageIndex = 0,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    Task<long> CountAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default);

    Task<bool> Exists(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default);
}