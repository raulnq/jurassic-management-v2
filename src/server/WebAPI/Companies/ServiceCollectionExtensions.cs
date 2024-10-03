namespace WebAPI.Companies;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCompanies(this IServiceCollection services, IConfiguration configuration)
    {
        var company = configuration.GetSection("Company").Get<Company>();

        if (company == null)
        {
            services.AddSingleton(new Company() { });

            return services;
        }

        services.AddSingleton(company!);

        return services;
    }
}