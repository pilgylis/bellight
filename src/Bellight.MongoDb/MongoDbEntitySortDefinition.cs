using System.Linq.Expressions;
using Bellight.DataManagement;
using MongoDB.Driver;

namespace Bellight.MongoDb;

internal class MongoDbEntitySortDefinition<TObject, TKey> : IEntitySortDefinition<TObject, TKey> 
    where TObject : IEntity<TKey>
    where TKey: IEquatable<TKey>
{
    private SortDefinition<TObject>? sort;
    public IEntitySortDefinition<TObject, TKey> Ascending(Expression<Func<TObject, object>> field)
    {
        if (sort is null)
        {
            var sortBuilder = Builders<TObject>.Sort;
            sort = sortBuilder.Ascending(field);
            return this;
        }

        sort = sort.Ascending(field);
        return this;
    }

    public IEntitySortDefinition<TObject, TKey> Descending(Expression<Func<TObject, object>> field)
    {
        if (sort is null)
        {
            var sortBuilder = Builders<TObject>.Sort;
            sort = sortBuilder.Descending(field);
            return this;
        }

        sort = sort.Descending(field);
        return this;
    }

    internal SortDefinition<TObject>? GetSort()
    {
        return sort;
    }
}