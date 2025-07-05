using Bellight.AutoMapper;
using Bellight.Core;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace ConsoleAutoMapper;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Demo auto mapper");

        var services = new ServiceCollection();

        services.AddBellightCore(options =>
        {
            options.DependencyCacheOptions.PrettyPrint = true;

            options.AddAutoMapper(cfg =>
            {
                cfg.CreateMap<string, decimal>().ConvertUsing(s => string.IsNullOrEmpty(s) ? default : Convert.ToDecimal(s));
                cfg.CreateMap<string, decimal?>().ConvertUsing(s => string.IsNullOrEmpty(s) ? null : (decimal?)Convert.ToDecimal(s));
                //cfg.CreateMap<decimal, string>().ConvertUsing(v => v.ToString());
            });
        });

        var serviceProvider = services.BuildServiceProvider();

        var mapperService = serviceProvider.GetService<IModelMappingService>();

        // forward
        var user1 = new User
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Johnny",
            Tel = "0987654321",
            Balance = 9000.90009m
        };
        var viewModel1 = mapperService.Map<UserViewModel>(user1);

        Console.WriteLine("Name: {0} - {1} - {2}", viewModel1.Name, viewModel1.Tel, viewModel1.Balance);

        // backward
        var viewModel2 = new UserViewModel
        {
            Name = "Cathy",
            Tel = "098888888",
            Balance = "9000.90009"
        };

        var user2 = mapperService.Map<User>(viewModel2);
        Console.WriteLine("Name: {0} - {1} - {2}", user2.Name, user2.Tel, user2.Balance);
    }
}