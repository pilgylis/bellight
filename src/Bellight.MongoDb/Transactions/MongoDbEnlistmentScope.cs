using MongoDB.Driver;
using System.Transactions;

namespace Bellight.MongoDb.Transactions;

public class MongoDbEnlistmentScope(
    IClientSessionHandle sessionHandle, 
    MongoDbEnlistmentScope.Unregister unregister) : IEnlistmentNotification
{
    public delegate void Unregister();

    private readonly Unregister _unregister = unregister;
    private readonly IClientSessionHandle _sessionHandle = sessionHandle;

    public void Commit(Enlistment enlistment)
    {
        try
        {
            _sessionHandle.CommitTransaction();
            enlistment.Done();
        }
        finally
        {
            _unregister();
        }
    }

    public void InDoubt(Enlistment enlistment)
    {
        try
        {
            _sessionHandle.AbortTransaction();
            enlistment.Done();
        }
        finally
        {
            _unregister();
        }
    }

    public void Prepare(PreparingEnlistment preparingEnlistment)
    {
        preparingEnlistment.Prepared();
    }

    public void Rollback(Enlistment enlistment)
    {
        try
        {
            _sessionHandle.AbortTransaction();
            enlistment.Done();
        }
        finally
        {
            _unregister();
        }
    }
}