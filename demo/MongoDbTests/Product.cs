using Bellight.MongoDb;

namespace MongoDbTests;

public class Product : MongoBaseEntity<string>
{
    public string? Name { get; set; }
    public decimal? Price { get; set; }
}