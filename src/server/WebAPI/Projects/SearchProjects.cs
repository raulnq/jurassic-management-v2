using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;

namespace WebAPI.Projects;

public static class SearchProjects
{
    public class Query
    {
        public Guid? ClientId { get; set; }
    }

    public class Result
    {
        public Guid ProjectId { get; set; }
        public Guid ClientId { get; set; }
        public string Name { get; set; } = default!;
    }

    public static async Task<RazorComponentResult> HandlePage(
    [AsParameters] Query query,
    [FromServices] SqlKataQueryRunner runner)
    {
        var result = await runner.List<Result>((qf) =>
        {
            var statement = qf.Query(Tables.Projects);
            if (query.ClientId.HasValue)
            {
                statement = statement.Where(Tables.Projects.Field(nameof(Project.ClientId)), query.ClientId);
            }
            return statement;
        });

        return new RazorComponentResult<SearchProjectsPage>(new { Result = result });
    }
}
