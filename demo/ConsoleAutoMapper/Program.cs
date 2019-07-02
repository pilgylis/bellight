using Bellight.AutoMapper;
using Bellight.Core;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace ConsoleAutoMapper
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Demo auto mapper");

            var services = new ServiceCollection();

            services.AddBellightCore(options => {
                options.DependencyCacheOptions.PrettyPrint = true;

                options.AddAutoMapper();
            });

            var serviceProvider = services.BuildServiceProvider();


            var mapperService = serviceProvider.GetService<IModelMappingService>();

            // forward
            var user1 = new User
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Johnny",
                Tel = "0987654321"
            };
            var viewModel1 = mapperService.Map<UserViewModel>(user1);

            Console.WriteLine("Name: {0} - {1}", viewModel1.Name, viewModel1.Tel);

            // backward
            var viewModel2 = new UserViewModel
            {
                Name = "Cathy",
                Tel = "098888888"
            };

            var user2 = mapperService.Map<User>(viewModel2);
            Console.WriteLine("Name: {0} - {1}", user2.Name, user2.Tel);
        }
    }
}
