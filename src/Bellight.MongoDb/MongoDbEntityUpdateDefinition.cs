using Bellight.DataManagement;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Bellight.MongoDb;

internal class MongoDbEntityUpdateDefinition<TObject, TKey> : IEntityUpdateDefinition<TObject, TKey> 
    where TObject : IEntity<TKey>
    where TKey: IEquatable<TKey>
{
    private UpdateDefinition<TObject>? update;

    public IEntityUpdateDefinition<TObject, TKey> Set<TField>(Expression<Func<TObject, TField>> field, TField fieldValue)
    {
        if (update is null)
        {
            var builder = Builders<TObject>.Update;
            update = builder.Set(field, fieldValue);
        }
        else
        {
            update = update.Set(field, fieldValue);
        }

        return this;
    }

    internal UpdateDefinition<TObject>? GetUpdate()
    {
        return update;
    }
}