namespace WebAPI.PayrollPayments;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPayrollPayments(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["AzureStorageConnectionString"];

        if (string.IsNullOrEmpty(connectionString))
        {
            return services;
        }

        services.AddSingleton(new PayrollPaymentStorage(connectionString));

        return services;
    }
}