using MongoDB.Driver;

namespace Bellight.MongoDb.Transactions
{
    public static class TransactionClientExtensions
    {
        public static IMongoClient AsTransactionClient(this IMongoClient collection)
        {
            return new MongoTransactionClient(collection);
        }
    }
}
