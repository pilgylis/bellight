using Bellight.MongoDb;

namespace MongoDbTests;

public class Customer : MongoBaseEntity<string>
{
    public string? Name { get; set; }
    public string? Email { get; set; }
}
