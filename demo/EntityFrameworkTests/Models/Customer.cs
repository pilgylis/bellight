using Bellight.DataManagement;

namespace EntityFrameworkTests.Models;

public class Customer : IEntity<int>
{
    public int Id { get; set; }
    public bool IsDeleted { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
}