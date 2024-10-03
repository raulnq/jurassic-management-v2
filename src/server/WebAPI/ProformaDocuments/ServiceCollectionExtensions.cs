using PostmarkDotNet;

namespace WebAPI.ProformaDocuments;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProformaDocuments(this IServiceCollection services, IConfiguration configuration)
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

        var connectionString = configuration["AzureStorageConnectionString"];

        if (string.IsNullOrEmpty(connectionString))
        {
            return services;
        }

        services.AddSingleton(new ProformaDocumentStorage(connectionString));

        var key = configuration["PostmarkKey"];

        if (string.IsNullOrEmpty(key))
        {
            return services;
        }

        services.AddTransient(x => new PostmarkClient(key));

        return services;
    }
}