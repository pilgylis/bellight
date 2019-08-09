# Bellight.Core
Provides automatic dependency registration for Microsoft.Extensions.DependencyInjection.

## Basic Usage

Define services that inherit the following interfaces for automatic dependency registration:

* ITransientDependency
* IScopedDependency
* ISingletonDependency

*Example:*
``` C#
public interface IMyService: ITransientDependency
{
    void DoSomething();
}

public class MyService: IMyService
{
    public void DoSomething()
    {
        Console.WriteLine("MyService - DoSomething");
    }
}

public class Program
{
    var services = new ServiceCollection();

    services.AddBellightCore(options => {
        options.DependencyCacheOptions.PrettyPrint = true;
    });

    var serviceProvider = services.BuildServiceProvider();

    var myService = serviceProvider.GetService<IMyService>();

    myService.DoSomething();
}
```

*Refer to examples for more sophisticated scenarios.*

## Plugins

&lt;TBD&gt;