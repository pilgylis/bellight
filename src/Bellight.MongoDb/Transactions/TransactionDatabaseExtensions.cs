using MongoDB.Driver;

namespace Bellight.MongoDb.Transactions;

public static class TransactionDatabaseExtensions
{
    public static IMongoDatabase AsTransactionDatabase(this IMongoDatabase collection)
    {
        return new MongoTransactionDatabase(collection);
    }
}