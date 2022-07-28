namespace Bellight.DataManagement;

internal class TransactionAccessor : ITransactionAccessor
{
    private ITransactionSession? session;

    public ITransactionSession? GetCurrentTransaction()
    {
        return session;
    }

    public void SetTransaction(ITransactionSession session)
    {
        this.session = session;
    }
}
