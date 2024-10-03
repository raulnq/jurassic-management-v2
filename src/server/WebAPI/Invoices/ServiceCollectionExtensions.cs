using Invoices;

namespace WebAPI.Invoices;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInvoices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["AzureStorageConnectionString"];

        if (string.IsNullOrEmpty(connectionString))
        {
            return services;
        }

        services.AddSingleton(new InvoiceStorage(connectionString));

        return services;
    }
}