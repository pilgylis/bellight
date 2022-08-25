using Bellight.MongoDb;
using Microsoft.Extensions.DependencyInjection;
using System.Transactions;

namespace MongoDbTests;

public class CounterTests : IClassFixture<MongoDbFixture>
{
    private readonly IMongoRepository<Counter, string> repository;
    private readonly MongoDbFixture fixture;
    private readonly CancellationTokenSource cancellationTokenSource = new();

    public CounterTests(MongoDbFixture fixture)
    {
        repository = fixture.Services.GetRequiredService<IMongoRepository<Counter, string>>();
        this.fixture = fixture;
    }

    [Fact]
    public async Task TransactionAbortTest()
    {
        using var scope = fixture.Services.CreateScope();
        var counterRepository = scope.ServiceProvider.GetRequiredService<IMongoRepository<Counter, string>>();
        await counterRepository.DeleteAsync(c => true, softDelete: false);
        var counter = (await counterRepository.FindAsync(c => true, pageIndex: 0, pageSize: 1)
            .ConfigureAwait(false)).FirstOrDefault();

        counter = new Counter
        {
            Value = 1
        };

        await counterRepository.AddAsync(counter)
            .ConfigureAwait(false);

        var itemId = counter.Id;

        var transactionScope = new TransactionScope();
        await counterRepository.UpdateAsync(itemId, update => update.Set(c => c.Value, 10));

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
        await counterRepository.DeleteAsync(c => true, softDelete: false);
        var counter = (await counterRepository.FindAsync(c => true, pageIndex: 0, pageSize: 1)
            .ConfigureAwait(false)).FirstOrDefault();

        counter = new Counter
        {
            Value = 1
        };

        await counterRepository.AddAsync(counter)
            .ConfigureAwait(false);

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