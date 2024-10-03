using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;

namespace WebAPI.Projects;

public static class GetProject
{
    public class Query
    {
        public Guid ProjectId { get; set; }
    }

    public class Result
    {
        public Guid ProjectId { get; set; }
        public Guid ClientId { get; set; }
        public string Name { get; set; } = default!;
    }

    public static async Task<Ok<Result>> Handle(
    [FromServices] SqlKataQueryRunner runner,
    [FromRoute] Guid clientId,
    [FromRoute] Guid projectId)
    {
        var result = await runner.Get<Result>((qf) => qf
                .Query(Tables.Projects)
                .Where(Tables.Projects.Field(nameof(Project.ProjectId)), projectId));

        return TypedResults.Ok(result);
    }
}
