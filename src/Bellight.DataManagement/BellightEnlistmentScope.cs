using System.Transactions;

namespace Bellight.DataManagement;

public class BellightEnlistmentScope : IEnlistmentNotification
{
    private readonly ITransactionAccessor transactionAccessor;
    private readonly Unregister unregister;

    public delegate void Unregister();

    public BellightEnlistmentScope(ITransactionAccessor transactionAccessor, Unregister unregister)
    {
        this.transactionAccessor = transactionAccessor;
        this.unregister = unregister;
    }

    public void Commit(Enlistment enlistment)
    {
        try
        {
            transactionAccessor.GetCurrentTransaction()?.Commit();
            enlistment.Done();
        }
        finally
        {
            unregister();
        }
    }

    public void InDoubt(Enlistment enlistment)
    {
        try
        {
            transactionAccessor.GetCurrentTransaction()?.Abort();
            enlistment.Done();
        }
        finally
        {
            unregister();
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
            transactionAccessor.GetCurrentTransaction()?.Abort();
            enlistment.Done();
        }
        finally
        {
            unregister();
        }
    }
}
