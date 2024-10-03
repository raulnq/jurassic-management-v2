using SqlKata.Execution;
using Microsoft.Data.SqlClient;
using SqlKata.Compilers;

namespace WebAPI.Infrastructure.SqlKata;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSqlKata(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped(_ =>
        {
            var connection = new SqlConnection(configuration["DbConnectionString"]);

            var compiler = new SqlServerCompiler() { UseLegacyPagination = false };

            var factory = new QueryFactory(connection, compiler);

            return factory;
        });

        services.AddScoped<SqlKataQueryRunner>();

        return services;
    }
}
