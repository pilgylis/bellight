using System.Linq.Expressions;
using Bellight.DataManagement;
using Microsoft.EntityFrameworkCore;

namespace Bellight.EntityFrameworkCore;

public class EntityFrameworkRepository<TObject, TKey>(DbContext context) : IRepository<TObject, TKey> 
    where TObject : class, IEntity<TKey>
    where TKey: IEquatable<TKey>
{
    public async Task AddAsync(TObject item, CancellationToken cancellationToken = default)
    {
        var set = context.Set<TObject>();
        set.Add(item);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddManyAsync(IEnumerable<TObject> items, CancellationToken cancellationToken = default)
    {
        var set = context.Set<TObject>();
        await set.AddRangeAsync(items, cancellationToken);
    }

    public async Task<long> CountAsync(Expression<Func<TObject, bool>> filter, CancellationToken cancellationToken = default)
    {
        var set = context.Set<TObject>();
        return await set.Where(filter).LongCountAsync(cancellationToken);
    }

    public async Task DeleteAsync(TKey id, bool softDelete = true, CancellationToken cancellationToken = default)
    {
        var set = context.Set<TObject>();
        if (softDelete)
        {
            await set.Where(m => m.Id.Equals(id))
                .ExecuteUpdateAsync(b => b.SetProperty(u => u.IsDeleted, true), cancellationToken: cancellationToken);

            return;
        }

        await set.Where(m => m.Id.Equals(id)).ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<long> DeleteManyAsync(Expression<Func<TObject, bool>> filter, bool softDelete = true, CancellationToken cancellationToken = default)
    {
        var set = context.Set<TObject>();
        if (softDelete)
        {
            return await set.Where(filter)
                .ExecuteUpdateAsync(b => b.SetProperty(u => u.IsDeleted, true), cancellationToken: cancellationToken);
        }

        return await set.Where(filter).ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<long> DeleteManyAsync(IEnumerable<TKey> ids, bool softDelete, CancellationToken token = default)
    {
        var set = context.Set<TObject>();
        if (softDelete)
        {
            return await set.Where(m => ids.Contains(m.Id))
                .ExecuteUpdateAsync(b => b.SetProperty(u => u.IsDeleted, true), cancellationToken: token);
        }

        return await set.Where(m => ids.Contains(m.Id)).ExecuteDeleteAsync(token);
    }

    public async Task<bool> Exists(Expression<Func<TObject, bool>> filter, CancellationToken cancellationToken = default)
    {
        var set = context.Set<TObject>();
        return await set.AnyAsync(filter, cancellationToken: cancellationToken);
    }

    public async Task<IEnumerable<TObject>> FindAsync(
        Expression<Func<TObject, bool>> filter,
        Expression<Func<IEntitySortDefinition<TObject, TKey>, IEntitySortDefinition<TObject, TKey>>>? sortOrders = null,
        int pageIndex = 0,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var set = context.Set<TObject>();

        var queryable = set.Where(filter);

        if (sortOrders is not null)
        {
            var sortDefinition = new EntitySortDefinition<TObject, TKey>();
            sortOrders.Compile().Invoke(sortDefinition);
            queryable = sortDefinition.Apply(queryable);
        }

        return await queryable.Skip(pageIndex * pageSize).Take(pageSize).ToListAsync(cancellationToken: cancellationToken);
    }

    public async Task<IEnumerable<TP>> FindAsync<TP>(
        Expression<Func<TObject, bool>> filter,
        Expression<Func<TObject, TP>> projection,
        Expression<Func<IEntitySortDefinition<TObject, TKey>, IEntitySortDefinition<TObject, TKey>>>? sortOrders = null,
        int pageIndex = 0,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var set = context.Set<TObject>();

        var queryable = set.Where(filter);

        if (sortOrders is not null)
        {
            var sortDefinition = new EntitySortDefinition<TObject, TKey>();
            sortOrders.Compile().Invoke(sortDefinition);
            queryable = sortDefinition.Apply(queryable);
        }

        return await queryable
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .Select(projection)
            .ToListAsync(cancellationToken: cancellationToken);
    }

    public async Task<TObject?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var set = context.Set<TObject>();
        return await set.FirstOrDefaultAsync(m => m.Id.Equals(id), cancellationToken: cancellationToken);
    }

    public Task ReplaceAsync(TKey id, TObject item, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public IQueryable<TObject> ToQueryable()
    {
        return context.Set<TObject>();
    }

    public async Task UpdateAsync(
        TKey id,
        Func<IEntityUpdateDefinition<TObject, TKey>, IEntityUpdateDefinition<TObject, TKey>> update,
        CancellationToken cancellationToken = default)
    {
        var updateDefinition = new EntityUpdateDefinition<TObject, TKey>();
        update.Invoke(updateDefinition);
        _ = await context.Set<TObject>().Where(m => m.Id.Equals(id)).AsNoTracking()
            .ExecuteUpdateAsync(updateDefinition.SetPropertyExpression, cancellationToken);
    }

    public async Task<long> UpdateManyAsync(
        Expression<Func<TObject, bool>> filter,
        Func<IEntityUpdateDefinition<TObject, TKey>, IEntityUpdateDefinition<TObject, TKey>> update,
        CancellationToken cancellationToken = default)
    {
        var updateDefinition = new EntityUpdateDefinition<TObject, TKey>();
        update.Invoke(updateDefinition);
        return await context.Set<TObject>().Where(filter).AsNoTracking()
            .ExecuteUpdateAsync(updateDefinition.SetPropertyExpression, cancellationToken);
    }
}