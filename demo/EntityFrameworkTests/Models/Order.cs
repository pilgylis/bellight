using Bellight.DataManagement;
namespace EntityFrameworkTests.Models;

public class Order : IEntity<int>
{
    public int Id { get; set; }
    public bool IsDeleted { get; set; }
    public int CustomerId { get; set; }
    public IEnumerable<OrderProduct> Products { get; set; } = [];
    public DateTime OrderDate { get; set; }
    public decimal DiscountAmount { get; set; } = 0;
    public decimal Total { get; set; }
}