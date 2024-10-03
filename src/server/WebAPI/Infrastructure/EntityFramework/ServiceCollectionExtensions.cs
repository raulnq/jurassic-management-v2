using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace WebAPI.Infrastructure.EntityFramework;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEntityFramework(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(configuration["DbConnectionString"])
            .ConfigureWarnings(w => w.Ignore(SqlServerEventId.SavepointsDisabledBecauseOfMARS)));

        services.AddScoped<TransactionBehavior>();

        return services;
    }
}
