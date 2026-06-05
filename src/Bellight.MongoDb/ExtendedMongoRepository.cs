
using Bellight.DataManagement;
using MongoDB.Driver;

namespace Bellight.MongoDb;
public class ExtendedMongoRepository<T>(ICollectionFactory collectionFactory) : MongoRepository<T, string>(collectionFactory), IMongoRepository<T> where T : class, IEntity<string>
{
    public ProjectionDefinitionBuilder<T> Project => Builders<T>.Projection;

    public UpdateDefinitionBuilder<T> Update => Builders<T>.Update;

    public SortDefinitionBuilder<T> Sort => Builders<T>.Sort;

    public FilterDefinitionBuilder<T> Filter => Builders<T>.Filter;

    public Task<long> CountAsync(FilterDefinition<T> filter, CancellationToken cancellationToken = default)
    {
        var collection = CollectionFactory.GetCollection<T>(GetObjectType()!);
        return collection.CountDocumentsAsync(Filter.And(FilterBase(), filter), null, cancellationToken);
    }

    public async Task<bool> Exists(FilterDefinition<T> filter, CancellationToken cancellationToken = default)
    {
        var count = await CountAsync(Filter.And(FilterBase(), filter), cancellationToken).ConfigureAwait(false);
        return count > 0;
    }

    public async Task<IEnumerable<T>> FindAsync(FilterDefinition<T> filter, SortDefinition<T>? sort = null, int pageIndex = 0, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var collection = CollectionFactory.GetCollection<T>(GetObjectType()!);
        var find = collection.Find(Filter.And(FilterBase(), filter));
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

    public async Task<IEnumerable<P>> FindAsync<P>(FilterDefinition<T> filter, System.Linq.Expressions.Expression<Func<T, P>> projection, SortDefinition<T>? sort = null, int pageIndex = 0, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var collection = CollectionFactory.GetCollection<T>(GetObjectType()!);
        var find = collection.Find(Filter.And(FilterBase(), filter));
        if (sort != null)
        {
            find = find.Sort(sort);
        }

        return await find
            .Skip(pageIndex * pageSize)
            .Limit(pageSize)
            .Project(projection)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public virtual async Task<long> UpdateManyAsync(
        FilterDefinition<T> filter,
        UpdateDefinition<T> update,
        CancellationToken cancellationToken = default)
    {
        UpdateDefinition<T> mUpdate = typeof(MongoTrackedEntity<string>).IsAssignableFrom(typeof(T)) ? Update.Combine(
            Update.Set(
                m => (m as MongoTrackedEntity<string>)!.UpdatedOnUtc,
                DateTime.UtcNow
            ),
            update
        ) : update;

        var collection = CollectionFactory.GetCollection<T>(GetObjectType()!);
        var updateResult = await collection.UpdateManyAsync(
            Filter.And(FilterBase(), filter),
            mUpdate,
            null,
            cancellationToken).ConfigureAwait(false);

        return updateResult.ModifiedCount;
    }

    public Task UpdateAsync(string id, UpdateDefinition<T> update, CancellationToken cancellationToken = default)
    {
        return UpdateManyAsync(Filter.Eq(e => e.Id, id), update, cancellationToken);
    }
}