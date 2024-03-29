﻿using System.Linq.Expressions;

namespace Bellight.DataManagement;

public interface IRepository<T, Tid> where T : class, IEntity<Tid>
{
    IQueryable<T> ToQueryable();

    Task AddAsync(T item, CancellationToken cancellationToken = default);

    Task AddManyAsync(IEnumerable<T> items, CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Tid id,
        IEntityUpdateDefinition<T> update,
        CancellationToken cancellationToken = default);

    Task<long> UpdateManyAsync(
        Expression<Func<T, bool>> filter,
        IEntityUpdateDefinition<T> update,
        CancellationToken cancellationToken = default);

    Task ReplaceAsync(Tid id, T item, CancellationToken cancellationToken = default);

    Task DeleteAsync(Tid id, bool softDelete = true, CancellationToken cancellationToken = default);

    Task<long> DeleteManyAsync(
        Expression<Func<T, bool>> filter,
        bool softDelete = true,
        CancellationToken cancellationToken = default);

    Task<long> DeleteManyAsync(IEnumerable<Tid> ids, bool softDelete, CancellationToken token = default);

    Task<T?> GetByIdAsync(Tid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> FindAsync(
        Expression<Func<T, bool>> filter,
        IEntitySortDefinition<T>? sortOrders = null,
        int pageIndex = 0,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<P>> FindAsync<P>(
        Expression<Func<T, bool>> filter,
        Expression<Func<T, P>> projection,
        IEntitySortDefinition<T>? sortOrders = null,
        int pageIndex = 0,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    Task<long> CountAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default);

    Task<bool> Exists(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default);

    IEntityUpdateDefinition<T> UpdateDefinition {
        get;
    }

    IEntitySortDefinition<T> SortDefinition {
        get;
    }
}