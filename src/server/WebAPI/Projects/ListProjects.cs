using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;

namespace WebAPI.Projects;

public static class ListProjects
{
    public class Query : ListQuery
    {
        public string? Name { get; set; }
        [JsonIgnore]
        public Guid? ClientId { get; set; }
    }

    public class Result
    {
        public Guid ProjectId { get; set; }
        public Guid ClientId { get; set; }
        public string Name { get; set; } = default!;
    }

    public class Runner : BaseRunner
    {
        public Runner(SqlKataQueryRunner queryRunner) : base(queryRunner) { }

        public Task<ListResults<Result>> Run(Query query)
        {
            return _queryRunner.List<Query, Result>((qf) =>
            {
                var statement = qf.Query(Tables.Projects);

                if (!string.IsNullOrEmpty(query.Name))
                {
                    statement = statement.WhereLike(Tables.Projects.Field(nameof(Project.Name)), $"%{query.Name}%");
                }
                if (query.ClientId.HasValue)
                {
                    statement = statement.Where(Tables.Projects.Field(nameof(Project.ClientId)), query.ClientId);
                }
                return statement;
            }, query);
        }
    }

    public static async Task<Ok<ListResults<Result>>> Handle(
        [FromServices] SqlKataQueryRunner runner,
        [FromRoute] Guid clientId,
        [AsParameters] Query query)
    {
        query.ClientId = clientId;

        return TypedResults.Ok(await new Runner(runner).Run(query));
    }

    public static async Task<RazorComponentResult> HandlePage(
        [AsParameters] Query query,
        [FromRoute] Guid clientId,
        [FromServices] SqlKataQueryRunner runner)
    {
        var result = await Handle(runner, clientId, query);

        return new RazorComponentResult<ListProjectsPage>(new { Result = result.Value, Query = query });
    }
}
