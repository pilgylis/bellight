using Microsoft.Extensions.DependencyInjection;

namespace Bellight.DataManagement;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTransaction(this IServiceCollection services)
    {
        return services.AddScoped<ITransactionFactory, TransactionFactory>()
            .AddScoped<ITransactionAccessor, TransactionAccessor>();
    }
}
