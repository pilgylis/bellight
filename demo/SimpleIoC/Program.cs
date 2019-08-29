using Bellight.Core;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace SimpleIoC
{
    class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection();

            services.AddBellightCore(options => {
                options.DependencyCacheOptions.PrettyPrint = true;
            });

            var serviceProvider = services.BuildServiceProvider();

            // resolve IService => only one is returned
            // cannot determine which one
            var singleInstance = serviceProvider.GetService<IService>();
            singleInstance.DoSomething();

            // resolve IEnumerable
            var items = serviceProvider.GetServices<IService>();
            foreach (var item in items)
            {
                item.DoSomething();
            }

            var keyedServiceFactory = serviceProvider.GetService<IKeyedServiceFactory>();
            var c1 = keyedServiceFactory.Resolve<IKeyedServiceC>("C1");
            c1.DoSomethingInKeyed();

            Console.ReadKey();
        }
    }
}
