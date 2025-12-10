using Bellight.AutoMapper;

namespace ConsoleAutoMapper;

[MappedModel(typeof(User))]
public class UserViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Tel { get; set; } = string.Empty;
    public string Balance { get; set; } = string.Empty;
}