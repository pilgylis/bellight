using Bellight.DataManagement;
using LinqKit;
using MongoDB.Driver;
using System.Linq.Expressions;
using System.Reflection;

namespace Bellight.MongoDb;

#pragma warning disable CS8602, CS8604 // Dereference of a possibly null reference.
public class MongoRepository<T, Tid> : IMongoRepository<T, Tid> where T : class, IEntity<Tid>
{
    private IMongoCollection<T>? _collection;

    public IMongoCollection<T> Collection
    {
        get
        {
            if (_collection != null) return _collection;
            _collection = CollectionFactory.GetCollection<T>(GetObjectType());
            return _collection;
        }
    }

    protected ICollectionFactory CollectionFactory { get; }
    protected ITransactionAccessor TransactionAccessor { get; }
    private string? _objectType;

    private IClientSessionHandle? session;

    public MongoRepository(ICollectionFactory collectionFactory, ITransactionAccessor transactionAccessor)
    {
        CollectionFactory = collectionFactory;
        TransactionAccessor = transactionAccessor;
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
        if (Session != null)
        {
            return await Collection.Find(Session, filter).FirstOrDefaultAsync(cancellationToken);
        }

        return await Collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<T>> FindAsync(FilterDefinition<T> filter, SortDefinition<T>? sort = null, int pageIndex = 0, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var find = Session != null ? Collection.Find(Session, Filter.And(FilterBase(), filter))
            : Collection.Find(Filter.And(FilterBase(), filter));
        if (sort != null)
        {
            find = find.Sort(sort);
        }

        return await find
            .Skip(pageIndex * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<P>> FindAsync<P>(FilterDefinition<T> filter, Expression<Func<T, P>> projection, SortDefinition<T>? sort = null, int pageIndex = 0, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var find = Session != null ? Collection.Find(Session, Filter.And(FilterBase(), filter))
            : Collection.Find(Filter.And(FilterBase(), filter));

        if (sort != null)
        {
            find = find.Sort(sort);
        }

        return await find.Project(projection).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<P>> FindAsync<P>(Expression<Func<T, bool>> filter, Expression<Func<T, P>> projection, IEnumerable<KeyValuePair<string, bool>>? sortOrders = null, int pageIndex = 0, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var predicate = PredicateBuilder.New<T>(true)
            .And(filter)
            .And(i => !i.IsDeleted);
        var find = Session != null ? Collection.Find(Session, predicate)
            : Collection.Find(predicate);
        var sort = CreateSortDefinition(sortOrders);
        if (sort != null)
        {
            find = find.Sort(sort);
        }

        return await find.Project(projection).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> filter, SortDefinition<T>? sort = null, int pageIndex = 0, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var predicate = PredicateBuilder.New<T>(true)
            .And(filter)
            .And(i => !i.IsDeleted);

        var find = Session != null ? Collection.Find(Session, predicate)
            : Collection.Find(predicate);

        if (sort != null)
        {
            find = find.Sort(sort);
        }

        return await find.ToListAsync(cancellationToken);
    }

    public Task<long> CountAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default)
    {
        var predicate = PredicateBuilder.New<T>(true)
            .And(filter)
            .And(i => !i.IsDeleted);
        return Session != null ? Collection.CountDocumentsAsync(Session, predicate, null, cancellationToken)
            : Collection.CountDocumentsAsync(predicate, null, cancellationToken);
    }

    public Task<long> CountAsync(FilterDefinition<T> filter, CancellationToken cancellationToken = default)
    {
        return Session != null ? Collection.CountDocumentsAsync(Session, Filter.And(FilterBase(), filter), null, cancellationToken)
            : Collection.CountDocumentsAsync(filter, null, cancellationToken);
    }

    public async Task<bool> Exists(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default)
    {
        var count = await CountAsync(filter, cancellationToken);
        return count > 0;
    }

    public async Task<bool> Exists(FilterDefinition<T> filter, CancellationToken cancellationToken = default)
    {
        var count = await CountAsync(filter, cancellationToken);
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

        if (Session != null)
        {
            await Collection.InsertOneAsync(Session, item, null, cancellationToken);
            return;
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

        if (Session != null)
        {
            await Collection.InsertManyAsync(Session, items, null, cancellationToken);
            return;
        }

        await Collection.InsertManyAsync(items, null, cancellationToken);
    }

    public Task UpdateAsync(Tid id, Func<EntityUpdateDefinition<T>, EntityUpdateDefinition<T>> updateFunc, CancellationToken cancellationToken = default)
    {
        var updateDefinition = new MongoDbEntityUpdateDefinition<T>();
        updateFunc.Invoke(updateDefinition);

        return UpdateAsync(id, updateDefinition.GetUpdate(), cancellationToken);
    }

    public virtual Task UpdateAsync(Tid id, UpdateDefinition<T> update, CancellationToken cancellationToken = default)
    {
        var filter = Filter.And(FilterBase(), Filter.Eq(m => m.Id, id));
        return UpdateManyAsync(filter, update, cancellationToken);
    }

    public Task UpdateAsync(Tid id, Func<UpdateDefinition<T>, UpdateDefinition<T>> updateFunc, CancellationToken cancellationToken = default)
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

        return Session != null ? Collection.UpdateOneAsync(Session, filter, update, null, cancellationToken)
            : Collection.UpdateOneAsync(filter, update, null, cancellationToken);
    }

    public async Task<long> UpdateManyAsync(Expression<Func<T, bool>> filter, UpdateDefinition<T> update, CancellationToken cancellationToken = default)
    {
        var task = Session != null ?
            Collection.UpdateOneAsync(Session, filter, update, cancellationToken: cancellationToken)
            : Collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);

        var updateResult = await task;
        return updateResult.ModifiedCount;
    }

    public async Task<long> UpdateManyAsync(Expression<Func<T, bool>> filter, Func<UpdateDefinition<T>, UpdateDefinition<T>> updateFunc, CancellationToken cancellationToken = default)
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

        var task = Session != null ? Collection.UpdateOneAsync(Session, filter, update, null, cancellationToken)
            : Collection.UpdateOneAsync(filter, update, null, cancellationToken);

        var updateResult = await task;
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
            throw new Exception("updateFunc must have a update.");
        }

        return UpdateManyAsync(filter, update, cancellationToken);
    }

    public virtual async Task<long> UpdateManyAsync(FilterDefinition<T> filter, UpdateDefinition<T> update, CancellationToken cancellationToken = default)
    {
        UpdateDefinition<T> mUpdate;
        if (typeof(MongoTrackedEntity<Tid>).IsAssignableFrom(typeof(T)))
        {
            mUpdate = Update.Combine(Update.Set(m => (m as MongoTrackedEntity<Tid>).UpdatedOnUtc, DateTime.UtcNow), update);
        }
        else
        {
            mUpdate = update;
        }

        var updateResult = await (Session != null ? Collection.UpdateManyAsync(Session, filter, mUpdate, null, cancellationToken)
            : Collection.UpdateManyAsync(filter, mUpdate, null, cancellationToken));

        return updateResult.ModifiedCount;
    }

    public virtual async Task<long> UpdateManyAsync(FilterDefinition<T> filter, Func<UpdateDefinition<T>, UpdateDefinition<T>> updateFunc, CancellationToken cancellationToken = default)
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

        var updateResult = await (Session != null ? Collection.UpdateManyAsync(Session, filter, mUpdate, null, cancellationToken)
            : Collection.UpdateManyAsync(filter, mUpdate, null, cancellationToken));

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

        return Session != null ? Collection.DeleteOneAsync(Session, filter, null, cancellationToken)
            : Collection.DeleteOneAsync(filter, cancellationToken);
    }

    public async Task<long> DeleteAsync(Expression<Func<T, bool>> filter, bool softDelete = true, CancellationToken cancellationToken = default)
    {
        if (!softDelete)
        {
            var deleteResult = await Collection.DeleteOneAsync(filter, cancellationToken);
            return deleteResult.DeletedCount;
        }

        var update = Update.Set(u => u.IsDeleted, true);

        if (typeof(MongoTrackedEntity<Tid>).IsAssignableFrom(typeof(T)))
        {
            update = update.Set(u => (u as MongoTrackedEntity<Tid>).UpdatedOnUtc, DateTime.UtcNow);
        }

        var deleteTask = Session != null ? Collection.UpdateManyAsync(Session, filter, update, null, cancellationToken)
            : Collection.UpdateManyAsync(filter, update, null, cancellationToken);

        return (await deleteTask).ModifiedCount;
    }

    public virtual async Task<long> DeleteManyAsync(IEnumerable<Tid> ids, bool softDelete,
        CancellationToken token = default)
    {
        var filter = Filter.And(FilterBase(), Filter.In(m => m.Id, ids));
        if (!softDelete)
        {
            var deleteManyResult = await Collection.DeleteManyAsync(filter, token);
            return deleteManyResult.DeletedCount;
        }

        var update = Update.Set(u => u.IsDeleted, true);

        if (typeof(MongoTrackedEntity<Tid>).IsAssignableFrom(typeof(T)))
        {
            update = update.Set(u => (u as MongoTrackedEntity<Tid>).UpdatedOnUtc, DateTime.UtcNow);
        }

        var updateTask = Session != null ? Collection.UpdateManyAsync(Session, filter, update, null, token)
            : Collection.UpdateManyAsync(filter, update, null, token);

        return (await updateTask).ModifiedCount;
    }

    public virtual async Task<bool> CheckExistence(string field, object value,
        CancellationToken token = default)
    {
        var filterDefinitions = new List<FilterDefinition<T>>
            {
                FilterBase(),
                Filter.Eq(field, value)
            };
        var documentCount = Session != null ?
            await Collection.CountDocumentsAsync(Session, Filter.And(filterDefinitions), null, token)
            : await Collection.CountDocumentsAsync(Filter.And(filterDefinitions), null, token);
        return documentCount > 0;
    }

    public Task ReplaceAsync(Tid id, T item, CancellationToken cancellationToken = default)
    {
        var filter = Filter.And(FilterBase(), Filter.Eq(m => m.Id, id));
        return Session != null ?
            Collection.ReplaceOneAsync(Session, filter, item, cancellationToken: cancellationToken)
            : Collection.ReplaceOneAsync(filter, item, cancellationToken: cancellationToken);
    }

    protected IClientSessionHandle? Session
    {
        get
        {
            if (session != null)
            {
                return session;
            }

            var transactionSession = TransactionAccessor.GetCurrentTransaction();
            if (transactionSession == null)
            {
                return null;
            }

            session = CollectionFactory.Database.Client.StartSession();
            session.StartTransaction();

            transactionSession.TransactionDispose += OnTransactionDispose;
            transactionSession.TransactionCommit += OnTransactionCommit;
            transactionSession.TransactionAbort += OnTransactionAbort;

            EnsureCollectionExists();

            return session;
        }
    }

    private void OnTransactionAbort()
    {
        session?.AbortTransaction();
        session?.Dispose();
        session = null;
    }

    private void OnTransactionCommit()
    {
        session?.CommitTransaction();
        session?.Dispose();
        session = null;
    }

    private void OnTransactionDispose()
    {
        session?.Dispose();
        session = null;
    }

    private void EnsureCollectionExists()
    {
        var database = CollectionFactory.Database;

        var collectionName = GetObjectType();

        var allCollections = database.ListCollectionNames().ToEnumerable();

        if (allCollections.Contains(collectionName))
        {
            return;
        }

        database.CreateCollection(collectionName);
    }

    protected string? GetObjectType()
    {
        if (!string.IsNullOrEmpty(_objectType)) return _objectType;

        var attribute = typeof(T).GetCustomAttribute<MongoCollectionAttribute>();

        _objectType = string.IsNullOrEmpty(attribute?.ObjectType) ?
            typeof(T).FullName?.Replace(".", "_")
            : attribute?.ObjectType;

        return _objectType;
    }
}
#pragma warning restore CS8602, CS8604 // Dereference of a possibly null reference.
