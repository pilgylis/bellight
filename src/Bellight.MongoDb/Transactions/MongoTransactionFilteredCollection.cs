using MongoDB.Driver;

namespace Bellight.MongoDb.Transactions
{
    public class MongoTransactionFilteredCollection<T>
        : MongoTransactionCollection<T>
        , IFilteredMongoCollection<T>
    {
        private readonly IFilteredMongoCollection<T> _filteredCollection;

        public MongoTransactionFilteredCollection(IFilteredMongoCollection<T> filteredCollection)
            : base(filteredCollection)
        {
            _filteredCollection = filteredCollection;
        }

        public FilterDefinition<T> Filter => _filteredCollection.Filter;
    }
}