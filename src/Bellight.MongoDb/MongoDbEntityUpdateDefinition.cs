using Bellight.DataManagement;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Bellight.MongoDb;

internal class MongoDbEntityUpdateDefinition<TObject> : EntityUpdateDefinition<TObject> where TObject : IEntity
{
    private UpdateDefinition<TObject>? update;

    public override EntityUpdateDefinition<TObject> Set<TField>(Expression<Func<TObject, TField>> field, TField fieldValue)
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

    public UpdateDefinition<TObject>? GetUpdate()
    {
        return update;
    }
}
