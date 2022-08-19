using Bellight.MongoDb;
using Microsoft.Extensions.DependencyInjection;

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

    private void CounterThread()
    {
        Task.Run(async () => {
            var cancellationToken = cancellationTokenSource.Token;
            using var scope = fixture.Services.CreateScope();
            var counterRepository = scope.ServiceProvider.GetRequiredService<IMongoRepository<Counter, string>>();
            var counter = (await counterRepository.FindAsync(c => true, pageIndex: 0, pageSize: 1)
                .ConfigureAwait(false)).FirstOrDefault();

            if (counter is null)
            {
                counter = new Counter
                {
                    Value = 1
                };

                await counterRepository.AddAsync(counter)
                    .ConfigureAwait(false);
            }

            var id = counter.Id;

            var value = 1;

            while (!cancellationToken.IsCancellationRequested)
            {

            }
        });
    }
}
