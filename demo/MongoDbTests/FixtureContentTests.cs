using Bellight.MongoDb;
using Microsoft.Extensions.DependencyInjection;

namespace MongoDbTests
{
    public class FixtureContentTests : IClassFixture<MongoDbFixture>
    {
        private readonly MongoDbFixture fixture;

        public FixtureContentTests(MongoDbFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public async Task DataExists()
        {
            var customerRepository = fixture.Services.GetRequiredService<IMongoRepository<Customer, string>>();
            var productRepository = fixture.Services.GetRequiredService<IMongoRepository<Product, string>>();
            var orderRepository = fixture.Services.GetRequiredService<IMongoRepository<Order, string>>();

            var orderCount = await orderRepository.CountAsync(o => true);

            Assert.True(orderCount > 0);
        }
    }
}