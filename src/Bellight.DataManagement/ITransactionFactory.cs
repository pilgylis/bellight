namespace Bellight.DataManagement;

public interface ITransactionFactory
{
    ITransactionSession CreateTransaction();
}
