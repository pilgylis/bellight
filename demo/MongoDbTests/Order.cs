using Bellight.MongoDb;

namespace MongoDbTests;

public class Order : MongoBaseEntity<string>
{
    public string CustomerId { get; set; } = string.Empty;
    public IEnumerable<OrderProduct> Products { get; set; } = Enumerable.Empty<OrderProduct>();
    public DateTime OrderDate { get; set; }
    public decimal DiscountAmount { get; set; } = 0;
    public decimal Total { get; set; }
}
