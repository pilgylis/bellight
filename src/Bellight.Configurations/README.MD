# Bellight.Configurations
A plugin of Bellight.Core that provides automatic configuration registration (Microsoft.Extensions.Configuration)

## Usage

1. Create configuration section in appsettings.json
``` json
  {
    "NestedProperty": {
      "NestedA": "abc",
      "NestedB": "123"
    }
  }
```
2. Create configuration model to be bound to, that implements *IAppSettingSection*

``` C#
  public class NestedProperty: IAppSettingSection
  {
      public string NestedA { get; set; }
      public string NestedB { get; set; }
  }
```

3. Register into AddBellightCore
``` C#
  var services = new ServiceCollection();

  services.AddBellightCore(options => 
    options.AddBellightConfigurations());
```