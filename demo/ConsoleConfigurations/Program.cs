using Bellight.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;
using Microsoft.Extensions.Options;

namespace ConsoleConfigurations
{
    class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection();

            services.AddBellightCore(options => options.AddBellightConfigurations());

            var serviceProvider = services.BuildServiceProvider();

            var configuration = serviceProvider.GetService<IConfiguration>();
            
            foreach (var pair in configuration.AsEnumerable())
            {
                Console.WriteLine("{0}: {1}", pair.Key, pair.Value);
            }

            var testOptions = serviceProvider.GetService<IOptions<NestedProperty>>();
            Console.WriteLine(testOptions.Value.NestedA);
        }
    }
}
