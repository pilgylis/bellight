using Microsoft.Extensions.DependencyInjection;
using System;

namespace SimpleIoC
{
    class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection();

            services.AddBellightCore(options => { });

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
        }
    }
}
