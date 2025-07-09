using Bellight.DataManagement;
using Bellight.MongoDb;
using Microsoft.Extensions.DependencyInjection;

namespace MongoDbTests;

public class FixtureContentTests(MongoDbFixture fixture) : IClassFixture<MongoDbFixture>
{

    [Fact]
    public async Task DataExists()
    {
        var customerRepository = fixture.Services.GetRequiredService<IRepository<Customer, string>>();
        var productRepository = fixture.Services.GetRequiredService<IRepository<Product, string>>();
        var orderRepository = fixture.Services.GetRequiredService<IRepository<Order, string>>();

        var orderCount = await orderRepository.CountAsync(o => true);

        Assert.True(orderCount > 0);
    }
}