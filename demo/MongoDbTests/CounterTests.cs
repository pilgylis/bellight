using Bellight.MongoDb;
using Microsoft.Extensions.DependencyInjection;
using System.Transactions;
using MongoDB.Driver;

namespace MongoDbTests;

public class CounterTests(MongoDbFixture fixture) : IClassFixture<MongoDbFixture>
{
    private readonly IMongoRepository<Counter, string> repository = fixture.Services.GetRequiredService<IMongoRepository<Counter, string>>();
    private readonly MongoDbFixture fixture = fixture;
    private readonly CancellationTokenSource cancellationTokenSource = new();

    [Fact]
    public async Task TransactionAbortTest()
    {
        using var scope = fixture.Services.CreateScope();
        var counterRepository = scope.ServiceProvider.GetRequiredService<IMongoRepository<Counter, string>>();
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
        using var scope = fixture.Services.CreateScope();
        var counterRepository = scope.ServiceProvider.GetRequiredService<IMongoRepository<Counter, string>>();
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