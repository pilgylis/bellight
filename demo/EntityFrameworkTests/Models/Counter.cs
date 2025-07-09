using Bellight.DataManagement;

namespace EntityFrameworkTests.Models;

public class Counter : IEntity<int>
{
    public int Value { get; set; } = 0;
    public int Id { get; set; }
    public bool IsDeleted { get; set; }
}