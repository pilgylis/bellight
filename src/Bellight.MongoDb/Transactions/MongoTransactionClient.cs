using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Clusters;
using System.Runtime.CompilerServices;

namespace Bellight.MongoDb.Transactions;

public sealed class MongoTransactionClient(IMongoClient client) : IMongoClient
{
    public ClientBulkWriteResult BulkWrite(IReadOnlyList<BulkWriteModel> models, ClientBulkWriteOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return TryGetSession(out var session) ? 
            client.BulkWrite(session, models, options, cancellationToken) 
            : client.BulkWrite(models, options, cancellationToken);
    }

    public ClientBulkWriteResult BulkWrite(IClientSessionHandle session, IReadOnlyList<BulkWriteModel> models,
        ClientBulkWriteOptions? options = null, CancellationToken cancellationToken = default)
    {
        return client.BulkWrite(models, options, cancellationToken);
    }

    public Task<ClientBulkWriteResult> BulkWriteAsync(IReadOnlyList<BulkWriteModel> models, ClientBulkWriteOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return TryGetSession(out var session) ? 
            client.BulkWriteAsync(session, models, options, cancellationToken) 
            : client.BulkWriteAsync(models, options, cancellationToken);
    }

    public Task<ClientBulkWriteResult> BulkWriteAsync(IClientSessionHandle session, IReadOnlyList<BulkWriteModel> models, ClientBulkWriteOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return client.BulkWriteAsync(models, options, cancellationToken);
    }

    public void DropDatabase(
        string name,
        CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out var session))
        {
            client.DropDatabase(session, name, cancellationToken);
            return;
        }

        client.DropDatabase(name, cancellationToken);
    }

    public void DropDatabase(
        IClientSessionHandle session,
        string name,
        CancellationToken cancellationToken = default)
    {
        client.DropDatabase(session, name, cancellationToken);
    }

    public Task DropDatabaseAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out var session))
        {
            return client.DropDatabaseAsync(session, name, cancellationToken);
        }

        return client.DropDatabaseAsync(name, cancellationToken);
    }

    public Task DropDatabaseAsync(
        IClientSessionHandle session,
        string name,
        CancellationToken cancellationToken = default)
    {
        return client.DropDatabaseAsync(session, name, cancellationToken);
    }

    public IMongoDatabase GetDatabase(string name, MongoDatabaseSettings? settings = null)
    {
        return client.GetDatabase(name, settings).AsTransactionDatabase();
    }

    public IAsyncCursor<string> ListDatabaseNames(CancellationToken cancellationToken = default)
    {
        return client.ListDatabaseNames(cancellationToken);
    }

    public IAsyncCursor<string> ListDatabaseNames(
        ListDatabaseNamesOptions options,
        CancellationToken cancellationToken = default)
    {
        return TryGetSession(out var session) ? 
            client.ListDatabaseNames(session, options, cancellationToken) 
            : client.ListDatabaseNames(options, cancellationToken);
    }

    public IAsyncCursor<string> ListDatabaseNames(
        IClientSessionHandle session,
        CancellationToken cancellationToken = default)
    {
        return client.ListDatabaseNames(session, cancellationToken);
    }

    public IAsyncCursor<string> ListDatabaseNames(
        IClientSessionHandle session,
        ListDatabaseNamesOptions options,
        CancellationToken cancellationToken = default)
    {
        return client.ListDatabaseNames(session, options, cancellationToken);
    }

    public Task<IAsyncCursor<string>> ListDatabaseNamesAsync(
        CancellationToken cancellationToken = default)
    {
        return TryGetSession(out var session) ? 
            client.ListDatabaseNamesAsync(session, cancellationToken) 
            : client.ListDatabaseNamesAsync(cancellationToken);
    }

    public Task<IAsyncCursor<string>> ListDatabaseNamesAsync(
        ListDatabaseNamesOptions options,
        CancellationToken cancellationToken = default)
    {
        return TryGetSession(out var session) ? 
            client.ListDatabaseNamesAsync(session, cancellationToken) 
            : client.ListDatabaseNamesAsync(options, cancellationToken);
    }

    public Task<IAsyncCursor<string>> ListDatabaseNamesAsync(
        IClientSessionHandle session,
        CancellationToken cancellationToken = default)
    {
        return client.ListDatabaseNamesAsync(session, cancellationToken);
    }

    public Task<IAsyncCursor<string>> ListDatabaseNamesAsync(
        IClientSessionHandle session,
        ListDatabaseNamesOptions options,
        CancellationToken cancellationToken = default)
    {
        return client.ListDatabaseNamesAsync(session, options, cancellationToken);
    }

    public IAsyncCursor<BsonDocument> ListDatabases(
        CancellationToken cancellationToken = default)
    {
        return TryGetSession(out var session) ? 
            client.ListDatabases(session, cancellationToken) 
            : client.ListDatabases(cancellationToken);
    }

    public IAsyncCursor<BsonDocument> ListDatabases(
        ListDatabasesOptions options,
        CancellationToken cancellationToken = default)
    {
        return TryGetSession(out var session) ? 
            client.ListDatabases(session, options, cancellationToken) 
            : client.ListDatabases(options, cancellationToken);
    }

    public IAsyncCursor<BsonDocument> ListDatabases(
        IClientSessionHandle session,
        CancellationToken cancellationToken = default)
    {
        return client.ListDatabases(session, cancellationToken);
    }

    public IAsyncCursor<BsonDocument> ListDatabases(
        IClientSessionHandle session,
        ListDatabasesOptions options,
        CancellationToken cancellationToken = default)
    {
        return client.ListDatabases(session, options, cancellationToken);
    }

    public Task<IAsyncCursor<BsonDocument>> ListDatabasesAsync(
        CancellationToken cancellationToken = default)
    {
        return TryGetSession(out var session) ? 
            client.ListDatabasesAsync(session, cancellationToken) 
            : client.ListDatabasesAsync(cancellationToken);
    }

    public Task<IAsyncCursor<BsonDocument>> ListDatabasesAsync(
        ListDatabasesOptions options,
        CancellationToken cancellationToken = default)
    {
        return TryGetSession(out var session) ? 
            client.ListDatabasesAsync(session, options, cancellationToken) 
            : client.ListDatabasesAsync(options, cancellationToken);
    }

    public Task<IAsyncCursor<BsonDocument>> ListDatabasesAsync(
        IClientSessionHandle session,
        CancellationToken cancellationToken = default)
    {
        return client.ListDatabasesAsync(session, cancellationToken);
    }

    public Task<IAsyncCursor<BsonDocument>> ListDatabasesAsync(
        IClientSessionHandle session,
        ListDatabasesOptions options,
        CancellationToken cancellationToken = default)
    {
        return client.ListDatabasesAsync(session, options, cancellationToken);
    }

    public IClientSessionHandle StartSession(
        ClientSessionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return client.StartSession(options, cancellationToken);
    }

    public Task<IClientSessionHandle> StartSessionAsync(
        ClientSessionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return client.StartSessionAsync(options, cancellationToken);
    }

    public IChangeStreamCursor<TResult> Watch<TResult>(
        PipelineDefinition<ChangeStreamDocument<BsonDocument>, TResult> pipeline,
        ChangeStreamOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return TryGetSession(out var session) ? 
            client.Watch(session, pipeline, options, cancellationToken) 
            : client.Watch(pipeline, options, cancellationToken);
    }

    public IChangeStreamCursor<TResult> Watch<TResult>(
        IClientSessionHandle session,
        PipelineDefinition<ChangeStreamDocument<BsonDocument>, TResult> pipeline,
        ChangeStreamOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return client.Watch(session, pipeline, options, cancellationToken);
    }

    public Task<IChangeStreamCursor<TResult>> WatchAsync<TResult>(
        PipelineDefinition<ChangeStreamDocument<BsonDocument>, TResult> pipeline,
        ChangeStreamOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return TryGetSession(out var session) ? 
            client.WatchAsync(session, pipeline, options, cancellationToken) 
            : client.WatchAsync(pipeline, options, cancellationToken);
    }

    public Task<IChangeStreamCursor<TResult>> WatchAsync<TResult>(
        IClientSessionHandle session,
        PipelineDefinition<ChangeStreamDocument<BsonDocument>, TResult> pipeline,
        ChangeStreamOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return client.WatchAsync(session, pipeline, options, cancellationToken);
    }

    public IMongoClient WithReadConcern(ReadConcern readConcern)
    {
        return client.WithReadConcern(readConcern).AsTransactionClient();
    }

    public IMongoClient WithReadPreference(ReadPreference readPreference)
    {
        return client.WithReadPreference(readPreference).AsTransactionClient();
    }

    public IMongoClient WithWriteConcern(WriteConcern writeConcern)
    {
        return client.WithWriteConcern(writeConcern).AsTransactionClient();
    }

    public ICluster Cluster => client.Cluster;

    public MongoClientSettings Settings => client.Settings;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool TryGetSession(out IClientSessionHandle sessionHandle) =>
        TransactionStore.TryGetSession(client, out sessionHandle);

    public void Dispose()
    {
        client.Dispose();
    }
}