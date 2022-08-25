using Bellight.MongoDb;

namespace MongoDbTests;

public class Counter : MongoBaseEntity<string>
{
    public int Value { get; set; } = 0;
}