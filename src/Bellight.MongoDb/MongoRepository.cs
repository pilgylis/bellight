using Bellight.DataManagement;
using LinqKit;
using MongoDB.Driver;
using System.Linq.Expressions;
using System.Reflection;

namespace Bellight.MongoDb;

#pragma warning disable CS8602, CS8604, RSC1202 // Dereference of a possibly null reference.
public class MongoRepository<T, Tid> : IMongoRepository<T, Tid> where T : class, IEntity<Tid>
{
    public IMongoCollection<T> Collection => CollectionFactory.GetCollection<T>(GetObjectType());

    protected ICollectionFactory CollectionFactory { get; }
    private string? _objectType;

    public MongoRepository(ICollectionFactory collectionFactory)
    {
        CollectionFactory = collectionFactory;
    }

    public FilterDefinitionBuilder<T> Filter => Builders<T>.Filter;
    public UpdateDefinitionBuilder<T> Update => Builders<T>.Update;

    public SortDefinitionBuilder<T> Sort => Builders<T>.Sort;
    public ProjectionDefinitionBuilder<T> Project => Builders<T>.Projection;

    public IQueryable<T> ToQueryable()
    {
        return Collection.AsQueryable();
    }

    public FilterDefinition<T> FilterBase()
    {
        return Filter.Ne(m => m.IsDeleted, true);
    }

    public void SetObjectType(string objectType)
    {
        _objectType = objectType;
    }

    public virtual async Task<T?> GetByIdAsync(Tid id, CancellationToken cancellationToken = default)
    {

        var filter = Filter.And(FilterBase(), Filter.Eq(m => m.Id, id));

        return await Collection.Find(filter)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IEnumerable<T>> FindAsync(
        FilterDefinition<T> filter,
        SortDefinition<T>? sort = null,
        int pageIndex = 0,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var find = Collection.Find(Filter.And(FilterBase(), filter));
        if (sort != null)
        {
            find = find.Sort(sort);
        }

        return await find
            .Skip(pageIndex * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<P>> FindAsync<P>(
        FilterDefinition<T> filter,
        Expression<Func<T, P>> projection,
        SortDefinition<T>? sort = null,
        int pageIndex = 0,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var find = Collection.Find(Filter.And(FilterBase(), filter));

        if (sort != null)
        {
            find = find.Sort(sort);
        }

        return await find.Project(projection)
            .Skip(pageIndex * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<P>> FindAsync<P>(
        Expression<Func<T, bool>> filter,
        Expression<Func<T, P>> projection,
        IEnumerable<KeyValuePair<string, bool>>? sortOrders = null,
        int pageIndex = 0,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var predicate = PredicateBuilder.New<T>(true)
            .And(filter)
            .And(i => !i.IsDeleted);
        var find = Collection.Find(predicate);
        var sort = CreateSortDefinition(sortOrders);
        if (sort != null)
        {
            find = find.Sort(sort);
        }

        return await find.Project(projection)
            .Skip(pageIndex * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<T>> FindAsync(
        Expression<Func<T, bool>> filter,
        SortDefinition<T>? sort = null,
        int pageIndex = 0,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var predicate = PredicateBuilder.New<T>(true)
            .And(filter)
            .And(i => !i.IsDeleted);

        var find = Collection.Find(predicate);

        if (sort != null)
        {
            find = find.Sort(sort);
        }

        return await find
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

    public Task<long> CountAsync(FilterDefinition<T> filter, CancellationToken cancellationToken = default)
    {
        return Collection.CountDocumentsAsync(Filter.And(FilterBase(), filter), null, cancellationToken);
    }

    public async Task<bool> Exists(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default)
    {
        var count = await CountAsync(Filter.And(FilterBase(), filter), cancellationToken).ConfigureAwait(false);
        return count > 0;
    }

    public async Task<bool> Exists(FilterDefinition<T> filter, CancellationToken cancellationToken = default)
    {
        var count = await CountAsync(Filter.And(FilterBase(), filter), cancellationToken).ConfigureAwait(false);
        return count > 0;
    }

    public virtual SortDefinition<T> CreateSortDefinition(IEnumerable<KeyValuePair<string, bool>>? sortOrders)
    {
        var builder = Builders<T>.Sort;
        if (sortOrders == null)
        {
            return builder.Combine();
        }

        var definitions = new List<SortDefinition<T>>();
        foreach (var pair in sortOrders)
        {
            var key = pair.Key;
            if (string.IsNullOrEmpty(key)) { continue; }
            key = key[..1].ToUpperInvariant() + key[1..];
            var definition = pair.Value ? builder.Ascending(key) : builder.Descending(key);
            definitions.Add(definition);
        }

        return builder.Combine(definitions);
    }

    public virtual async Task AddAsync(T item, CancellationToken cancellationToken = default)
    {
        if (item is MongoTrackedEntity<Tid>)
        {
            var mItem = item as MongoTrackedEntity<Tid>;
            mItem.CreatedOnUtc = DateTime.UtcNow;
            mItem.UpdatedOnUtc = DateTime.UtcNow;
        }

        await Collection.InsertOneAsync(item, null, cancellationToken);
    }

    public virtual async Task AddManyAsync(IEnumerable<T> items, CancellationToken cancellationToken = default)
    {
        items.ForEach(item =>
        {
            if (item is MongoTrackedEntity<Tid>)
            {
                var mItem = item as MongoTrackedEntity<Tid>;
                mItem.CreatedOnUtc = DateTime.UtcNow;
                mItem.UpdatedOnUtc = DateTime.UtcNow;
            }
        });

        await Collection.InsertManyAsync(items, null, cancellationToken).ConfigureAwait(false);
    }

    public Task UpdateAsync(
        Tid id,
        Func<EntityUpdateDefinition<T>, EntityUpdateDefinition<T>> updateFunc,
        CancellationToken cancellationToken = default)
    {
        var updateDefinition = new MongoDbEntityUpdateDefinition<T>();
        updateFunc.Invoke(updateDefinition);

        return UpdateAsync(id, updateDefinition.GetUpdate(), cancellationToken);
    }

    public virtual Task UpdateAsync(
        Tid id,
        UpdateDefinition<T> update,
        CancellationToken cancellationToken = default)
    {
        var filter = Filter.And(FilterBase(), Filter.Eq(m => m.Id, id));
        return UpdateManyAsync(filter, update, cancellationToken);
    }

    public Task UpdateAsync(
        Tid id,
        Func<UpdateDefinition<T>, UpdateDefinition<T>> updateFunc,
        CancellationToken cancellationToken = default)
    {
        var filter = Filter.And(FilterBase(), Filter.Eq(m => m.Id, id));

        UpdateDefinition<T> update;
        if (typeof(MongoTrackedEntity<Tid>).IsAssignableFrom(typeof(T)))
        {
            update = updateFunc.Invoke(Update.Set(m => (m as MongoTrackedEntity<Tid>).UpdatedOnUtc, DateTime.UtcNow));
        }
        else
        {
            update = updateFunc.Invoke(Update.Set(m => m.IsDeleted, false));
        }

        return Collection.UpdateOneAsync(filter, update, null, cancellationToken);
    }

    public async Task<long> UpdateManyAsync(
        Expression<Func<T, bool>> filter,
        UpdateDefinition<T> update,
        CancellationToken cancellationToken = default)
    {
        var task = Collection.UpdateOneAsync(Filter.And(FilterBase(), filter), update, cancellationToken: cancellationToken);

        var updateResult = await task.ConfigureAwait(false);
        return updateResult.ModifiedCount;
    }

    public async Task<long> UpdateManyAsync(
        Expression<Func<T, bool>> filter,
        Func<UpdateDefinition<T>, UpdateDefinition<T>> updateFunc,
        CancellationToken cancellationToken = default)
    {
        UpdateDefinition<T> update;
        if (typeof(MongoTrackedEntity<Tid>).IsAssignableFrom(typeof(T)))
        {
            update = updateFunc.Invoke(Update.Set(m => (m as MongoTrackedEntity<Tid>).UpdatedOnUtc, DateTime.UtcNow));
        }
        else
        {
            update = updateFunc.Invoke(Update.Set(m => m.IsDeleted, false));
        }

        var task = Collection.UpdateOneAsync(Filter.And(FilterBase(), filter), update, null, cancellationToken);

        var updateResult = await task.ConfigureAwait(false);
        return updateResult.ModifiedCount;
    }

    public Task<long> UpdateManyAsync(
        Expression<Func<T, bool>> filter,
        Func<EntityUpdateDefinition<T>, EntityUpdateDefinition<T>> updateFunc,
        CancellationToken cancellationToken = default)
    {

        var updateDefinition = new MongoDbEntityUpdateDefinition<T>();
        updateFunc.Invoke(updateDefinition);
        var update = updateDefinition.GetUpdate();

        if (update is null)
        {
            throw new BellightDataException("updateFunc must have an update.");
        }

        return UpdateManyAsync(filter, update, cancellationToken);
    }

    public virtual async Task<long> UpdateManyAsync(
        FilterDefinition<T> filter,
        UpdateDefinition<T> update,
        CancellationToken cancellationToken = default)
    {
        UpdateDefinition<T> mUpdate;
        if (typeof(MongoTrackedEntity<Tid>).IsAssignableFrom(typeof(T)))
        {
            mUpdate = Update.Combine(
                Update.Set(
                    m => (m as MongoTrackedEntity<Tid>).UpdatedOnUtc,
                    DateTime.UtcNow
                ),
                update
            );
        }
        else
        {
            mUpdate = update;
        }

        var updateResult = await Collection.UpdateManyAsync(
                Filter.And(FilterBase(), filter),
                mUpdate,
                null,
                cancellationToken).ConfigureAwait(false);

        return updateResult.ModifiedCount;
    }

    public virtual async Task<long> UpdateManyAsync(
        FilterDefinition<T> filter,
        Func<UpdateDefinition<T>, UpdateDefinition<T>> updateFunc,
        CancellationToken cancellationToken = default)
    {
        UpdateDefinition<T> mUpdate;
        if (typeof(MongoTrackedEntity<Tid>).IsAssignableFrom(typeof(T)))
        {
            mUpdate = updateFunc.Invoke(Update.Set(m => (m as MongoTrackedEntity<Tid>).UpdatedOnUtc, DateTime.UtcNow));
        }
        else
        {
            mUpdate = updateFunc.Invoke(Update.Set(m => m.IsDeleted, false));
        }

        var updateResult = await Collection.UpdateManyAsync(filter, mUpdate, null, cancellationToken)
            .ConfigureAwait(false);

        return updateResult.ModifiedCount;
    }

    public virtual Task DeleteAsync(Tid id, bool softDelete = true, CancellationToken cancellationToken = default)
    {
        if (softDelete)
        {
            var update = Update.Set(u => u.IsDeleted, true);

            if (typeof(MongoTrackedEntity<Tid>).IsAssignableFrom(typeof(T)))
            {
                update = update.Set(u => (u as MongoTrackedEntity<Tid>).UpdatedOnUtc, DateTime.UtcNow);
            }

            return UpdateAsync(id, update, cancellationToken);
        }

        var filter = Filter.And(FilterBase(), Filter.Eq(m => m.Id, id));

        return Collection.DeleteOneAsync(filter, cancellationToken);
    }

    public async Task<long> DeleteAsync(
        Expression<Func<T, bool>> filter,
        bool softDelete = true,
        CancellationToken cancellationToken = default)
    {
        if (!softDelete)
        {
            var deleteResult = await Collection.DeleteManyAsync(filter, cancellationToken).ConfigureAwait(false);
            return deleteResult.DeletedCount;
        }

        var update = Update.Set(u => u.IsDeleted, true);

        if (typeof(MongoTrackedEntity<Tid>).IsAssignableFrom(typeof(T)))
        {
            update = update.Set(u => (u as MongoTrackedEntity<Tid>).UpdatedOnUtc, DateTime.UtcNow);
        }

        var deleteTask = Collection.UpdateManyAsync(filter, update, null, cancellationToken);

        return (await deleteTask.ConfigureAwait(false)).ModifiedCount;
    }

    public virtual async Task<long> DeleteManyAsync(IEnumerable<Tid> ids, bool softDelete,
        CancellationToken token = default)
    {
        var filter = Filter.And(FilterBase(), Filter.In(m => m.Id, ids));
        if (!softDelete)
        {
            var deleteManyResult = await Collection.DeleteManyAsync(filter, token).ConfigureAwait(false);
            return deleteManyResult.DeletedCount;
        }

        var update = Update.Set(u => u.IsDeleted, true);

        if (typeof(MongoTrackedEntity<Tid>).IsAssignableFrom(typeof(T)))
        {
            update = update.Set(u => (u as MongoTrackedEntity<Tid>).UpdatedOnUtc, DateTime.UtcNow);
        }

        var updateTask = Collection.UpdateManyAsync(filter, update, null, token);

        return (await updateTask.ConfigureAwait(false)).ModifiedCount;
    }

    public virtual async Task<bool> CheckExistence(string field, object value,
        CancellationToken token = default)
    {
        var filterDefinitions = new List<FilterDefinition<T>>
            {
                FilterBase(),
                Filter.Eq(field, value)
            };
        var documentCount = await Collection.CountDocumentsAsync(
            Filter.And(filterDefinitions),
            null,
            token).ConfigureAwait(false);
        return documentCount > 0;
    }

    public Task ReplaceAsync(Tid id, T item, CancellationToken cancellationToken = default)
    {
        var filter = Filter.And(FilterBase(), Filter.Eq(m => m.Id, id));
        return Collection.ReplaceOneAsync(filter, item, cancellationToken: cancellationToken);
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
