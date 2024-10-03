using Transactions;

namespace WebAPI.Transactions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTransactions(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["AzureStorageConnectionString"];

        if (string.IsNullOrEmpty(connectionString))
        {
            return services;
        }

        services.AddSingleton(new TransactionStorage(connectionString));

        return services;
    }
}