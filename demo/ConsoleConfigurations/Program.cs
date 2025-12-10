using Bellight.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace ConsoleConfigurations;

internal class Program
{
    private static void Main(string[] args)
    {
        var services = new ServiceCollection();

        services.AddBellightCore(options => options.AddBellightConfigurations());

        var serviceProvider = services.BuildServiceProvider();

        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        foreach (var pair in configuration.AsEnumerable())
        {
            Console.WriteLine("{0}: {1}", pair.Key, pair.Value);
        }

        var testOptions = serviceProvider.GetRequiredService<IOptions<NestedProperty>>();
        Console.WriteLine(testOptions.Value.NestedA);
    }
}