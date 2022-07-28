namespace Bellight.MongoDb;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class MongoCollectionAttribute : Attribute
{
    public string? ObjectType { get; set; }

    public MongoCollectionAttribute() { }

    public MongoCollectionAttribute(string objectType)
    {
        ObjectType = objectType;
    }
}
