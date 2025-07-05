using Bellight.Configurations;

namespace ConsoleConfigurations;

public class NestedProperty : IAppSettingSection
{
    public string NestedA { get; set; }
    public string NestedB { get; set; }
}