using Bellight.DataManagement;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Bellight.MongoDb;

public abstract class MongoBaseEntity<TKey> : IEntity<TKey>
    where TKey: IEquatable<TKey>
{
    [BsonRepresentation(BsonType.ObjectId)]
    public TKey Id { get; set; } = default!;

    public bool IsDeleted { get; set; }
}