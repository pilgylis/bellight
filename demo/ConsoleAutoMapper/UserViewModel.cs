using Bellight.AutoMapper;

namespace ConsoleAutoMapper
{
    [MappedModel(typeof(User))]
    public class UserViewModel
    {
        public string Name { get; set; }
        public string Tel { get; set; }
        public string Balance { get; set; }
    }
}
