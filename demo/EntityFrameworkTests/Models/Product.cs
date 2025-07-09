
using Bellight.DataManagement;

namespace EntityFrameworkTests.Models;
public class Product : IEntity<int>
{
    public int Id { get; set; }
    public bool IsDeleted { get; set; }
    public string? Name { get; set; }
    public decimal? Price { get; set; }
}