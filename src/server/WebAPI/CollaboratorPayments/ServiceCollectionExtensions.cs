using CollaboratorPayments;

namespace WebAPI.CollaboratorPayments;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCollaboratorPayments(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["AzureStorageConnectionString"];

        if (string.IsNullOrEmpty(connectionString))
        {
            return services;
        }

        services.AddSingleton(new CollaboratorPaymentStorage(connectionString));

        return services;
    }
}