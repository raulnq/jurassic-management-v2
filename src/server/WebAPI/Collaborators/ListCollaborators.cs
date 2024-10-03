using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;

namespace WebAPI.Collaborators;

public static class ListCollaborators
{
    public class Query : ListQuery
    {
        public string? Name { get; set; }
    }

    public class Result
    {
        public Guid CollaboratorId { get; set; }
        public string Name { get; set; } = default!;
        public decimal WithholdingPercentage { get; set; }
    }

    public static async Task<Ok<ListResults<Result>>> Handle(
        [FromServices] SqlKataQueryRunner runner,
        [AsParameters] Query query)
    {
        var result = await runner.List<Query, Result>((qf) =>
        {
            var statement = qf.Query(Tables.Collaborators);

            if (!string.IsNullOrEmpty(query.Name))
            {
                statement = statement.WhereLike(Tables.Collaborators.Field(nameof(Collaborator.Name)), $"%{query.Name}%");
            }
            return statement;
        }, query);

        return TypedResults.Ok(result);
    }

    public static async Task<RazorComponentResult> HandlePage(
        [AsParameters] Query query,
        [FromServices] SqlKataQueryRunner runner)
    {
        var result = await Handle(runner, query);

        return new RazorComponentResult<ListCollaboratorsPage>(new { Result = result.Value, Query = query });
    }
}
