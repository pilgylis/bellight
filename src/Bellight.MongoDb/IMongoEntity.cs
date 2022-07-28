using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Bellight.MongoDb;

public interface IMongoEntity<IdType>
{
    IdType? Id { get; set; }
    bool IsDeleted { get; set; }
}

public abstract class MongoBaseEntity<IdType> : IMongoEntity<IdType>
{
    [BsonRepresentation(BsonType.ObjectId)]
    public IdType? Id { get; set; }
    public bool IsDeleted { get; set; }
}
