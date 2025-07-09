using System.Transactions;
using Bellight.DataManagement;
using EntityFrameworkTests.Models;

namespace EntityFrameworkTests;

public class CounterTests(EntityFrameworkFixture fixture) : IClassFixture<EntityFrameworkFixture>
{
    [Fact]
    public async Task TransactionAbortTest()
    {
        var serviceProvider = await fixture.GetServiceProviderAsync();
        var counterRepository = serviceProvider.GetRequiredService<IRepository<Counter, int>>();
        await counterRepository.DeleteManyAsync(c => true, softDelete: false);
        var counter = (await counterRepository.FindAsync(
            c => true, 
            pageIndex: 0, 
            pageSize: 1)).FirstOrDefault();

        counter = new Counter
        {
            Value = 1
        };

        await counterRepository.AddAsync(counter);

        var itemId = counter.Id;

        var transactionScope = new TransactionScope();
        await counterRepository.UpdateAsync(
            itemId, 
            update => update.Set(c => c.Value, 10));

        transactionScope.Dispose();

        var item = await counterRepository.GetByIdAsync(itemId);

        Assert.NotNull(item);
        Assert.Equal(1, item.Value);
    }

    [Fact]
    public async Task TransactionCommitTest()
    {
        var serviceProvider = await fixture.GetServiceProviderAsync();
        var counterRepository = serviceProvider.GetRequiredService<IRepository<Counter, int>>();
        await counterRepository.DeleteManyAsync(c => true, softDelete: false);
        var counter = (await counterRepository.FindAsync(
            c => true, 
            pageIndex: 0, 
            pageSize: 1)).FirstOrDefault();

        counter = new Counter
        {
            Value = 1
        };

        await counterRepository.AddAsync(counter);

        var itemId = counter.Id;

        using var transactionScope = new TransactionScope();
        await counterRepository.UpdateAsync(itemId, update => update.Set(c => c.Value, 10));

        transactionScope.Complete();
        transactionScope?.Dispose();

        var item = await counterRepository.GetByIdAsync(itemId);

        Assert.NotNull(item);
        Assert.Equal(10, item.Value);
    }
}