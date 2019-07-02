# bellight
Provides automatic dependency registration for Microsoft.Extensions.DependencyInjection.

## Installation
1. Add the package
``` sh
dotnet add package Bellight.Core
```
2. Initiate the extension (e.g. in Startup.cs of ASP.NET Core)
``` C#
var services = new ServiceCollection();

services.AddBellightCore(options => {
});
```
