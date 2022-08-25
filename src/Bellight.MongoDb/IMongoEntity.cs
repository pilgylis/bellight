using Bellight.DataManagement;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Bellight.MongoDb;

public abstract class MongoBaseEntity<IdType> : IEntity<IdType>
{
    [BsonRepresentation(BsonType.ObjectId)]
    public IdType? Id { get; set; }

    public bool IsDeleted { get; set; }
}