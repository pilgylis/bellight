using MongoDB.Bson.Serialization.Attributes;

namespace Bellight.MongoDb;

[BsonIgnoreExtraElements]
public abstract class MongoTrackedEntity<IdType> : MongoBaseEntity<IdType>
{
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? CreatedOnUtc { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? UpdatedOnUtc { get; set; }

    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    public virtual void SetUpdatedBy(string updatedBy)
    {
        UpdatedBy = updatedBy;

        UpdatedOnUtc = DateTime.UtcNow;
    }

    public virtual void SetCreateBy(string createBy)
    {
        CreatedBy = createBy;

        CreatedOnUtc = DateTime.UtcNow;
    }
}