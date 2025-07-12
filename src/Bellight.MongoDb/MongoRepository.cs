using Bellight.DataManagement;
using LinqKit;
using MongoDB.Driver;
using System.Linq.Expressions;
using System.Reflection;

namespace Bellight.MongoDb;

#pragma warning disable CS8602, CS8604, RSC1202 // Dereference of a possibly null reference.
public class MongoRepository<T, TKey>(ICollectionFactory collectionFactory) : IRepository<T, TKey>
    where T : class, IEntity<TKey>
    where TKey: IEquatable<TKey>
{
    public IMongoCollection<T> Collection => CollectionFactory.GetCollection<T>(GetObjectType());

    protected ICollectionFactory CollectionFactory { get; } = collectionFactory;
    private string? _objectType;

    public IQueryable<T> ToQueryable()
    {
        return Collection.AsQueryable();
    }

    public FilterDefinition<T> FilterBase()
    {
        return Builders<T>.Filter.Ne(m => m.IsDeleted, true);
    }

    public async Task AddAsync(T item, CancellationToken cancellationToken = default)
    {
        if (item is MongoTrackedEntity<TKey> mItem)
        {
            mItem.CreatedOnUtc = DateTime.UtcNow;
            mItem.UpdatedOnUtc = DateTime.UtcNow;
        }

        await Collection.InsertOneAsync(item, null, cancellationToken);
    }

    public async Task AddManyAsync(IEnumerable<T> items, CancellationToken cancellationToken = default)
    {
        items.ForEach(item =>
        {
            if (item is MongoTrackedEntity<TKey> mItem)
            {
                mItem.CreatedOnUtc = DateTime.UtcNow;
                mItem.UpdatedOnUtc = DateTime.UtcNow;
            }
        });

        await Collection.InsertManyAsync(items, null, cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAsync(
        TKey id,
        Func<IEntityUpdateDefinition<T, TKey>, IEntityUpdateDefinition<T, TKey>> update,
        CancellationToken cancellationToken = default)
    {
        var updateDefinition = new MongoDbEntityUpdateDefinition<T, TKey>();
        update(updateDefinition);

        await Collection.UpdateOneAsync(Builders<T>.Filter.Eq(m => m.Id, id), updateDefinition.GetUpdate(), cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task<long> UpdateManyAsync(
        Expression<Func<T, bool>> filter,
        Func<IEntityUpdateDefinition<T, TKey>, IEntityUpdateDefinition<T, TKey>> update,
        CancellationToken cancellationToken = default)
    {
        var updateDefinition = new MongoDbEntityUpdateDefinition<T, TKey>();
        update(updateDefinition);

        var result = await Collection.UpdateManyAsync(filter, updateDefinition.GetUpdate(), cancellationToken: cancellationToken).ConfigureAwait(false);

        return result.ModifiedCount;
    }

    public Task DeleteAsync(TKey id, bool softDelete = true, CancellationToken cancellationToken = default)
    {
        if (softDelete)
        {
            var update = Builders<T>.Update.Set(u => u.IsDeleted, true);

            if (typeof(MongoTrackedEntity<TKey>).IsAssignableFrom(typeof(T)))
            {
                update = update.Set(u => (u as MongoTrackedEntity<TKey>).UpdatedOnUtc, DateTime.UtcNow);
            }

            return Collection.UpdateOneAsync(Builders<T>.Filter.Eq(m => m.Id, id), update, null, cancellationToken);
        }

        var filter = Builders<T>.Filter.And(FilterBase(), Builders<T>.Filter.Eq(m => m.Id, id));

        return Collection.DeleteOneAsync(filter, cancellationToken);
    }

    public async Task<long> DeleteManyAsync(Expression<Func<T, bool>> filter, bool softDelete = true, CancellationToken cancellationToken = default)
    {
        if (!softDelete)
        {
            var deleteResult = await Collection.DeleteManyAsync(filter, cancellationToken).ConfigureAwait(false);
            return deleteResult.DeletedCount;
        }

        var update = Builders<T>.Update.Set(u => u.IsDeleted, true);

        if (typeof(MongoTrackedEntity<TKey>).IsAssignableFrom(typeof(T)))
        {
            update = update.Set(u => (u as MongoTrackedEntity<TKey>).UpdatedOnUtc, DateTime.UtcNow);
        }

        var deleteTask = Collection.UpdateManyAsync(filter, update, null, cancellationToken);

        return (await deleteTask.ConfigureAwait(false)).ModifiedCount;
    }

    public async Task<long> DeleteManyAsync(IEnumerable<TKey> ids, bool softDelete, CancellationToken token = default)
    {
        var filter = Builders<T>.Filter.And(FilterBase(), Builders<T>.Filter.In(m => m.Id, ids));
        if (!softDelete)
        {
            var deleteManyResult = await Collection.DeleteManyAsync(filter, token).ConfigureAwait(false);
            return deleteManyResult.DeletedCount;
        }

        var update = Builders<T>.Update.Set(u => u.IsDeleted, true);

        if (typeof(MongoTrackedEntity<TKey>).IsAssignableFrom(typeof(T)))
        {
            update = update.Set(u => (u as MongoTrackedEntity<TKey>).UpdatedOnUtc, DateTime.UtcNow);
        }

        var updateTask = Collection.UpdateManyAsync(filter, update, null, token);

        return (await updateTask.ConfigureAwait(false)).ModifiedCount;
    }

    public async Task<T?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<T>.Filter.And(FilterBase(), Builders<T>.Filter.Eq(m => m.Id, id));

        return await Collection.Find(filter)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IEnumerable<T>> FindAsync(
        Expression<Func<T, bool>> filter,
        Expression<Func<IEntitySortDefinition<T, TKey>, IEntitySortDefinition<T, TKey>>>? sortOrders = null,
        int pageIndex = 0,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var predicate = PredicateBuilder.New<T>(true)
            .And(filter)
            .And(i => !i.IsDeleted);

        var find = Collection.Find(predicate);
        
        if (sortOrders is not null)
        {
            var sortDefinition = new MongoDbEntitySortDefinition<T, TKey>();
            sortOrders.Compile().Invoke(sortDefinition);
            find = find.Sort(sortDefinition.GetSort());
        }

        return await find
            .Skip(pageIndex * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<P>> FindAsync<P>(
        Expression<Func<T, bool>> filter,
        Expression<Func<T, P>> projection,
        Expression<Func<IEntitySortDefinition<T, TKey>, IEntitySortDefinition<T, TKey>>>? sortOrders = null,
        int pageIndex = 0,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var predicate = PredicateBuilder.New<T>(true)
            .And(filter)
            .And(i => !i.IsDeleted);

        var find = Collection.Find(predicate);
        
        if (sortOrders is not null)
        {
            var sortDefinition = new MongoDbEntitySortDefinition<T, TKey>();
            sortOrders.Compile().Invoke(sortDefinition);
            find = find.Sort(sortDefinition.GetSort());
        }

        return await find.Project(projection)
            .Skip(pageIndex * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public Task<long> CountAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default)
    {
        var predicate = PredicateBuilder.New<T>(true)
            .And(filter)
            .And(i => !i.IsDeleted);
        return Collection.CountDocumentsAsync(predicate, null, cancellationToken);
    }

    public async Task<bool> Exists(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default)
    {
        var predicate = PredicateBuilder.New<T>(true)
            .And(filter)
            .And(i => !i.IsDeleted);
        var count = await CountAsync(predicate, cancellationToken).ConfigureAwait(false);
        return count > 0;
    }


    public Task ReplaceAsync(TKey id, T item, CancellationToken cancellationToken = default)
    {
        var filter = Builders<T>.Filter.And(FilterBase(), Builders<T>.Filter.Eq(m => m.Id, id));
        return Collection.ReplaceOneAsync(filter, item, cancellationToken: cancellationToken);
    }

    public void SetObjectType(string objectType)
    {
        _objectType = objectType;
    }

    protected string? GetObjectType()
    {
        if (!string.IsNullOrEmpty(_objectType)) { return _objectType; }

        var attribute = typeof(T).GetCustomAttribute<MongoCollectionAttribute>();

        _objectType = string.IsNullOrEmpty(attribute?.ObjectType) ?
            typeof(T).FullName?.Replace(".", "_")
            : attribute?.ObjectType;

        return _objectType;
    }
}
#pragma warning restore CS8602, CS8604, RSC1202 // Dereference of a possibly null reference.
