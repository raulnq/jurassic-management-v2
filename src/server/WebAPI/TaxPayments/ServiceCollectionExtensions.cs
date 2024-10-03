namespace WebAPI.TaxPayments;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTaxPayments(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["AzureStorageConnectionString"];

        if (string.IsNullOrEmpty(connectionString))
        {
            return services;
        }

        services.AddSingleton(new TaxPaymentStorage(connectionString));

        return services;
    }
}