namespace Bellight.DataManagement;

public class TransactionFactory : ITransactionFactory
{
    private readonly ITransactionAccessor transactionAccessor;

    public TransactionFactory(ITransactionAccessor transactionAccessor)
    {
        this.transactionAccessor = transactionAccessor;
    }

    public ITransactionSession CreateTransaction()
    {
        var session = new TransactionSession();
        transactionAccessor.SetTransaction(session);
        return session;
    }
}
