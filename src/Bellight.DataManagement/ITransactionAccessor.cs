namespace Bellight.DataManagement;

public interface ITransactionAccessor
{
    ITransactionSession? GetCurrentTransaction();
    void SetTransaction(ITransactionSession session);
}
