using Bellight.MongoDb;
using Microsoft.Extensions.DependencyInjection;
using System.Transactions;
using MongoDB.Driver;
using Bellight.DataManagement;
using Xunit.Abstractions;

namespace MongoDbTests;

public class CounterTests(MongoDbFixture fixture, ITestOutputHelper testOutputHelper) : IClassFixture<MongoDbFixture>
{
    [Fact]
    public async Task TransactionAbortTest()
    {
        var serviceProvider = await fixture.GetServiceProviderAsync(testOutputHelper);
        using var scope = serviceProvider.CreateScope();
        var counterRepository = scope.ServiceProvider.GetRequiredService<IRepository<Counter, string>>();
        await counterRepository.DeleteManyAsync(c => true, softDelete: false);
        var counter = (await counterRepository.FindAsync(
            c => true, 
            pageIndex: 0, 
            pageSize: 1)).FirstOrDefault();

        Assert.Null(counter);

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
        var serviceProvider = await fixture.GetServiceProviderAsync(testOutputHelper);
        using var scope = serviceProvider.CreateScope();
        var counterRepository = scope.ServiceProvider.GetRequiredService<IRepository<Counter, string>>();
        await counterRepository.DeleteManyAsync(c => true, softDelete: false);
        var counter = (await counterRepository.FindAsync(
            c => true, 
            pageIndex: 0, 
            pageSize: 1)).FirstOrDefault();

        Assert.Null(counter);
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