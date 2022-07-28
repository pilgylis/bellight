namespace Bellight.DataManagement;


internal class TransactionSession : ITransactionSession
{
    public event TransactionEventHandler? TransactionCommit;
    public event TransactionEventHandler? TransactionAbort;
    public event TransactionEventHandler? TransactionDispose;

    public void Abort()
    {
        TransactionAbort?.Invoke();
    }

    public void Commit()
    {
        TransactionCommit?.Invoke();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        TransactionDispose?.Invoke();
    }
}
