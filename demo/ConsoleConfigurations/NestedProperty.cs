using Bellight.Configurations;

namespace ConsoleConfigurations;

public class NestedProperty : IAppSettingSection
{
    public string NestedA { get; set; } = string.Empty;
    public string NestedB { get; set; } = string.Empty;
}