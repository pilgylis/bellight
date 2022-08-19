using System.Transactions;

namespace Bellight.DataManagement;

public class BellightEnlistmentScope : IEnlistmentNotification
{
    private readonly ITransactionFactory transactionFactory;
    private readonly Unregister unregister;

    public delegate void Unregister();

    private ITransactionSession? session;

    public BellightEnlistmentScope(ITransactionFactory transactionFactory, Unregister unregister)
    {
        this.transactionFactory = transactionFactory;
        this.unregister = unregister;
    }

    public void Commit(Enlistment enlistment)
    {
        try
        {
            session?.Commit();
            enlistment.Done();
        }
        finally
        {
            session?.Dispose();
            session = null;
            unregister();
        }
    }

    public void InDoubt(Enlistment enlistment)
    {
        try
        {
            session?.Abort();
            enlistment.Done();
        }
        finally
        {
            session?.Dispose();
            session = null;
            unregister();
        }
    }

    public void Prepare(PreparingEnlistment preparingEnlistment)
    {
        session = transactionFactory.CreateTransaction();
        preparingEnlistment.Prepared();
    }

    public void Rollback(Enlistment enlistment)
    {
        try
        {
            session?.Abort();
            enlistment.Done();
        }
        finally
        {
            session?.Dispose();
            session = null;
            unregister();
        }
    }
}
