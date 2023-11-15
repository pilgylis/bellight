using MongoDB.Driver;

namespace Bellight.MongoDb.Transactions;

public class MongoTransactionFilteredCollection<T>(IFilteredMongoCollection<T> filteredCollection)
    : MongoTransactionCollection<T>(filteredCollection)
    , IFilteredMongoCollection<T>
{
    private readonly IFilteredMongoCollection<T> _filteredCollection = filteredCollection;

    public FilterDefinition<T> Filter => _filteredCollection.Filter;
}