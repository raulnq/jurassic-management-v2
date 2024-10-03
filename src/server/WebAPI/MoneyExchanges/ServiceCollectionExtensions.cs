using MoneyExchanges;

namespace WebAPI.MoneyExchanges;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMoneyExchanges(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["AzureStorageConnectionString"];

        if (string.IsNullOrEmpty(connectionString))
        {
            return services;
        }

        services.AddSingleton(new MoneyExchangeStorage(connectionString));

        return services;
    }
}