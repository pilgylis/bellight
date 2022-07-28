namespace Bellight.DataManagement;

public delegate void TransactionEventHandler();
public interface ITransactionSession : IDisposable
{
    public event TransactionEventHandler? TransactionCommit;
    public event TransactionEventHandler? TransactionAbort;
    public event TransactionEventHandler? TransactionDispose;
    void Commit();
    void Abort();
}
