using Bellight.DataManagement;
using EntityFrameworkTests.Models;
using Json.More;
using Xunit.Abstractions;

namespace EntityFrameworkTests;

public class QueryTests(EntityFrameworkFixture fixture, ITestOutputHelper testOutputHelper) : IClassFixture<EntityFrameworkFixture>
{
    [Fact]
    public async Task QueryOrdersTest()
    {
        var serviceProvider = await fixture.GetServiceProviderAsync();
        var orderRepository = serviceProvider.GetRequiredService<IRepository<Order, int>>();
        var customerRepository = serviceProvider.GetRequiredService<IRepository<Customer, int>>();

        var firstCustomer = (await customerRepository.FindAsync(c => true, sortOrders: o => o.Ascending(c => c.Name), pageIndex: 0, pageSize: 1, cancellationToken: default))
            .FirstOrDefault()!;

        testOutputHelper.WriteLine("Customer: {0}", firstCustomer.ToJsonDocument().RootElement.GetRawText());

        var customerOrders = await orderRepository.FindAsync(o => o.CustomerId == firstCustomer.Id,
            sortOrders: s => s.Descending(o => o.OrderDate),
            pageIndex: 0,
            pageSize: 20,
            cancellationToken: default);

        testOutputHelper.WriteLine("Orders of this customer:");
        foreach (var order in customerOrders)
        {
            testOutputHelper.WriteLine(order.ToJsonDocument().RootElement.GetRawText());
        }

        Assert.NotEmpty(customerOrders);
    }
}
