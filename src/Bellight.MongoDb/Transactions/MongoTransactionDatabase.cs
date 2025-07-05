using MongoDB.Bson;
using MongoDB.Driver;
using System.Runtime.CompilerServices;

namespace Bellight.MongoDb.Transactions;

public class MongoTransactionDatabase(IMongoDatabase database) : IMongoDatabase
{
    public IAsyncCursor<TResult> Aggregate<TResult>(
        PipelineDefinition<NoPipelineInput, TResult> pipeline,
        AggregateOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out var session))
        {
            return Aggregate(session, pipeline, options, cancellationToken);
        }

        return database.Aggregate(pipeline, options, cancellationToken);
    }

    public IAsyncCursor<TResult> Aggregate<TResult>(
        IClientSessionHandle session,
        PipelineDefinition<NoPipelineInput, TResult> pipeline,
        AggregateOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return database.Aggregate(session, pipeline, options, cancellationToken);
    }

    public Task<IAsyncCursor<TResult>> AggregateAsync<TResult>(
        PipelineDefinition<NoPipelineInput, TResult> pipeline,
        AggregateOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out var session))
        {
            return database.AggregateAsync(session, pipeline, options, cancellationToken);
        }

        return database.AggregateAsync(pipeline, options, cancellationToken);
    }

    public Task<IAsyncCursor<TResult>> AggregateAsync<TResult>(
        IClientSessionHandle session,
        PipelineDefinition<NoPipelineInput, TResult> pipeline,
        AggregateOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return database.AggregateAsync(session, pipeline, options, cancellationToken);
    }

    public void AggregateToCollection<TResult>(
        PipelineDefinition<NoPipelineInput, TResult> pipeline,
        AggregateOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out var session))
        {
            database.AggregateToCollection(session, pipeline, options, cancellationToken);
            return;
        }

        database.AggregateToCollection(pipeline, options, cancellationToken);
    }

    public void AggregateToCollection<TResult>(
        IClientSessionHandle session,
        PipelineDefinition<NoPipelineInput, TResult> pipeline,
        AggregateOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        database.AggregateToCollection(session, pipeline, options, cancellationToken);
    }

    public Task AggregateToCollectionAsync<TResult>(
        PipelineDefinition<NoPipelineInput, TResult> pipeline,
        AggregateOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out var session))
        {
            return database.AggregateToCollectionAsync(session, pipeline, options, cancellationToken);
        }

        return database.AggregateToCollectionAsync(pipeline, options, cancellationToken);
    }

    public Task AggregateToCollectionAsync<TResult>(
        IClientSessionHandle session,
        PipelineDefinition<NoPipelineInput, TResult> pipeline,
        AggregateOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return database.AggregateToCollectionAsync(session, pipeline, options, cancellationToken);
    }

    public void CreateCollection(
        string name,
        CreateCollectionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out var session))
        {
            database.CreateCollection(session, name, options, cancellationToken);
            return;
        }

        database.CreateCollection(name, options, cancellationToken);
    }

    public void CreateCollection(
        IClientSessionHandle session,
        string name,
        CreateCollectionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        database.CreateCollection(session, name, options, cancellationToken);
    }

    public Task CreateCollectionAsync(
        string name,
        CreateCollectionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out var session))
        {
            return database.CreateCollectionAsync(session, name, options, cancellationToken);
        }

        return database.CreateCollectionAsync(name, options, cancellationToken);
    }

    public Task CreateCollectionAsync(
        IClientSessionHandle session,
        string name,
        CreateCollectionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return database.CreateCollectionAsync(session, name, options, cancellationToken);
    }

    public void CreateView<TDocument, TResult>(
        string viewName,
        string viewOn,
        PipelineDefinition<TDocument, TResult> pipeline,
        CreateViewOptions<TDocument>? options = null,
        CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out var session))
        {
            database.CreateView(session, viewName, viewOn, pipeline, options, cancellationToken);
            return;
        }

        database.CreateView(viewName, viewOn, pipeline, options, cancellationToken);
    }

    public void CreateView<TDocument, TResult>(
        IClientSessionHandle session,
        string viewName,
        string viewOn,
        PipelineDefinition<TDocument, TResult> pipeline,
        CreateViewOptions<TDocument>? options = null,
        CancellationToken cancellationToken = default)
    {
        database.CreateView(session, viewName, viewOn, pipeline, options, cancellationToken);
    }

    public Task CreateViewAsync<TDocument, TResult>(
        string viewName,
        string viewOn,
        PipelineDefinition<TDocument, TResult> pipeline,
        CreateViewOptions<TDocument>? options = null,
        CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out var session))
        {
            return database.CreateViewAsync(
                session,
                viewName,
                viewOn,
                pipeline,
                options,
                cancellationToken);
        }

        return database.CreateViewAsync(viewName, viewOn, pipeline, options, cancellationToken);
    }

    public Task CreateViewAsync<TDocument, TResult>(
        IClientSessionHandle session,
        string viewName,
        string viewOn,
        PipelineDefinition<TDocument, TResult> pipeline,
        CreateViewOptions<TDocument>? options = null,
        CancellationToken cancellationToken = default)
    {
        return database.CreateViewAsync(session, viewName, viewOn, pipeline, options, cancellationToken);
    }

    public void DropCollection(string name, CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out var session))
        {
            database.DropCollection(session, name, cancellationToken);
            return;
        }

        database.DropCollection(name, cancellationToken);
    }

    public void DropCollection(
        IClientSessionHandle session,
        string name,
        CancellationToken cancellationToken = default)
    {
        database.DropCollection(session, name, cancellationToken);
    }

    public void DropCollection(string name, DropCollectionOptions options, CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out var session))
        {
            database.DropCollection(session, name, options, cancellationToken);
            return;
        }

        database.DropCollection(name, options, cancellationToken);
    }

    public void DropCollection(IClientSessionHandle session, string name, DropCollectionOptions options, CancellationToken cancellationToken = default)
    {
        database.DropCollection(session, name, options, cancellationToken);
    }

    public Task DropCollectionAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out var session))
        {
            return database.DropCollectionAsync(session, name, cancellationToken);
        }

        return database.DropCollectionAsync(name, cancellationToken);
    }

    public Task DropCollectionAsync(
        IClientSessionHandle session,
        string name,
        CancellationToken cancellationToken = default)
    {
        return database.DropCollectionAsync(session, name, cancellationToken);
    }

    public Task DropCollectionAsync(string name, DropCollectionOptions options, CancellationToken cancellationToken = default)
    {
        return database.DropCollectionAsync(name, options, cancellationToken);
    }

    public Task DropCollectionAsync(IClientSessionHandle session, string name, DropCollectionOptions options, CancellationToken cancellationToken = default)
    {
        return database.DropCollectionAsync(session, name, options, cancellationToken);
    }

    public IMongoCollection<TDocument> GetCollection<TDocument>(
        string name,
        MongoCollectionSettings? settings = null)
    {
        return database.GetCollection<TDocument>(name, settings).AsTransactionCollection();
    }

    public IAsyncCursor<string> ListCollectionNames(
        ListCollectionNamesOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out var session))
        {
            return database.ListCollectionNames(session, options, cancellationToken);
        }

        return database.ListCollectionNames(options, cancellationToken);
    }

    public IAsyncCursor<string> ListCollectionNames(
        IClientSessionHandle session,
        ListCollectionNamesOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return database.ListCollectionNames(session, options, cancellationToken);
    }

    public Task<IAsyncCursor<string>> ListCollectionNamesAsync(
        ListCollectionNamesOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out var session))
        {
            return database.ListCollectionNamesAsync(session, options, cancellationToken);
        }

        return database.ListCollectionNamesAsync(options, cancellationToken);
    }

    public Task<IAsyncCursor<string>> ListCollectionNamesAsync(
        IClientSessionHandle session,
        ListCollectionNamesOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return database.ListCollectionNamesAsync(session, options, cancellationToken);
    }

    public IAsyncCursor<BsonDocument> ListCollections(
        ListCollectionsOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out var session))
        {
            return database.ListCollections(session, options, cancellationToken);
        }

        return database.ListCollections(options, cancellationToken);
    }

    public IAsyncCursor<BsonDocument> ListCollections(
        IClientSessionHandle session,
        ListCollectionsOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return database.ListCollections(session, options, cancellationToken);
    }

    public Task<IAsyncCursor<BsonDocument>> ListCollectionsAsync(
        ListCollectionsOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out var session))
        {
            return database.ListCollectionsAsync(session, options, cancellationToken);
        }

        return database.ListCollectionsAsync(options, cancellationToken);
    }

    public Task<IAsyncCursor<BsonDocument>> ListCollectionsAsync(
        IClientSessionHandle session,
        ListCollectionsOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return database.ListCollectionsAsync(session, options, cancellationToken);
    }

    public void RenameCollection(
        string oldName,
        string newName,
        RenameCollectionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out var session))
        {
            database.RenameCollection(session, oldName, newName, options, cancellationToken);
            return;
        }

        database.RenameCollection(oldName, newName, options, cancellationToken);
    }

    public void RenameCollection(
        IClientSessionHandle session,
        string oldName,
        string newName,
        RenameCollectionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        database.RenameCollection(session, oldName, newName, options, cancellationToken);
    }

    public Task RenameCollectionAsync(
        string oldName,
        string newName,
        RenameCollectionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out var session))
        {
            return database.RenameCollectionAsync(session, oldName, newName, options, cancellationToken);
        }

        return database.RenameCollectionAsync(oldName, newName, options, cancellationToken);
    }

    public Task RenameCollectionAsync(
        IClientSessionHandle session,
        string oldName,
        string newName,
        RenameCollectionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return database.RenameCollectionAsync(session, oldName, newName, options, cancellationToken);
    }

    public TResult RunCommand<TResult>(
        Command<TResult> command,
        ReadPreference? readPreference = null,
        CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out var session))
        {
            return database.RunCommand(session, command, readPreference, cancellationToken);
        }

        return database.RunCommand(command, readPreference, cancellationToken);
    }

    public TResult RunCommand<TResult>(
        IClientSessionHandle session,
        Command<TResult> command,
        ReadPreference? readPreference = null,
        CancellationToken cancellationToken = default)
    {
        return database.RunCommand(session, command, readPreference, cancellationToken);
    }

    public Task<TResult> RunCommandAsync<TResult>(
        Command<TResult> command,
        ReadPreference? readPreference = null,
        CancellationToken cancellationToken = default)
    {
        return database.RunCommandAsync(command, readPreference, cancellationToken);
    }

    public Task<TResult> RunCommandAsync<TResult>(
        IClientSessionHandle session,
        Command<TResult> command,
        ReadPreference? readPreference = null,
        CancellationToken cancellationToken = default)
    {
        return database.RunCommandAsync(session, command, readPreference, cancellationToken);
    }

    public IChangeStreamCursor<TResult> Watch<TResult>(
        PipelineDefinition<ChangeStreamDocument<BsonDocument>, TResult> pipeline,
        ChangeStreamOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out var session))
        {
            return database.Watch(session, pipeline, options, cancellationToken);
        }

        return database.Watch(pipeline, options, cancellationToken);
    }

    public IChangeStreamCursor<TResult> Watch<TResult>(
        IClientSessionHandle session,
        PipelineDefinition<ChangeStreamDocument<BsonDocument>, TResult> pipeline,
        ChangeStreamOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return database.Watch(session, pipeline, options, cancellationToken);
    }

    public Task<IChangeStreamCursor<TResult>> WatchAsync<TResult>(
        PipelineDefinition<ChangeStreamDocument<BsonDocument>, TResult> pipeline,
        ChangeStreamOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (TryGetSession(out var session))
        {
            return database.WatchAsync(session, pipeline, options, cancellationToken);
        }

        return database.WatchAsync(pipeline, options, cancellationToken);
    }

    public Task<IChangeStreamCursor<TResult>> WatchAsync<TResult>(
        IClientSessionHandle session,
        PipelineDefinition<ChangeStreamDocument<BsonDocument>, TResult> pipeline,
        ChangeStreamOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return database.WatchAsync(session, pipeline, options, cancellationToken);
    }

    public IMongoDatabase WithReadConcern(ReadConcern readConcern)
    {
        return database.WithReadConcern(readConcern).AsTransactionDatabase();
    }

    public IMongoDatabase WithReadPreference(ReadPreference readPreference)
    {
        return database.WithReadPreference(readPreference).AsTransactionDatabase();
    }

    public IMongoDatabase WithWriteConcern(WriteConcern writeConcern)
    {
        return database.WithWriteConcern(writeConcern).AsTransactionDatabase();
    }

    public IMongoClient Client => database.Client;

    public DatabaseNamespace DatabaseNamespace => database.DatabaseNamespace;

    public MongoDatabaseSettings Settings => database.Settings;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool TryGetSession(out IClientSessionHandle sessionHandle) =>
        TransactionStore.TryGetSession(database.Client, out sessionHandle);
}