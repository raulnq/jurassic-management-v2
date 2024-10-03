using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;

namespace WebAPI.ClientContacts;

public static class ListClientContacts
{
    public class Query : ListQuery
    {
        [JsonIgnore]
        public Guid? ClientId { get; set; }
    }

    public class Result
    {
        public Guid ClientContactId { get; set; }
        public Guid ClientId { get; set; }
        public string Name { get; set; } = default!;
        public string Email { get; set; } = default!;
    }

    public class Runner : BaseRunner
    {
        public Runner(SqlKataQueryRunner queryRunner) : base(queryRunner) { }

        public Task<ListResults<Result>> Run(Query query)
        {
            return _queryRunner.List<Query, Result>((qf) =>
            {
                var statement = qf.Query(Tables.ClientContacts);
                if (query.ClientId.HasValue)
                {
                    statement = statement.Where(Tables.ClientContacts.Field(nameof(ClientContact.ClientId)), query.ClientId);
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

        return new RazorComponentResult<ListClientContactsPage>(new { Result = result.Value, Query = query });
    }
}
