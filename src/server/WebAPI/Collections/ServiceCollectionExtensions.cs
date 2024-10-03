using Collections;

namespace WebAPI.Collections;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCollections(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["AzureStorageConnectionString"];

        if (string.IsNullOrEmpty(connectionString))
        {
            return services;
        }

        services.AddSingleton(new CollectionStorage(connectionString));

        return services;
    }
}