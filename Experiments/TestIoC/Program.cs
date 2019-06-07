using System.Linq;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace TestIoC
{
    class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddTransient<IService, ServiceA>();
            services.AddTransient<IService, ServiceB>();

            var serviceProvider = services.BuildServiceProvider();
            var instance = serviceProvider.GetService<IService>();

            Console.WriteLine(instance.GetType().Name);

            var allItems = serviceProvider.GetServices<IService>();
            
            Console.WriteLine(allItems.LongCount());
            foreach (var item in allItems)
            {
                Console.WriteLine(item.GetType().Name);
            }
        }
    }

    public interface IService {}
    public class ServiceA: IService {}
    public class ServiceB: IService {}
}
