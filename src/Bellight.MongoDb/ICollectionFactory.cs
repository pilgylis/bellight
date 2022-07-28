using MongoDB.Driver;

namespace Bellight.MongoDb;

public interface ICollectionFactory
{
    IMongoCollection<T> GetCollection<T>(string collectionName) where T : class;
    IMongoDatabase Database { get; }
}