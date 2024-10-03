using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Clients;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;

namespace WebAPI.Collections;

public static class ListCollections
{
    public class Query : ListQuery
    {
        public string? Status { get; set; }

        public Guid? ClientId { get; set; }
    }

    public class Result
    {
        public Guid CollectionId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public string? ClientName { get; set; }
        public decimal Total { get; set; }
        public decimal ITF { get; set; }
        public string? Status { get; set; }
        public string? Currency { get; set; }
        public decimal Commission { get; set; }
        public string? Number { get; set; }
    }

    public static async Task<Ok<ListResults<Result>>> Handle(
    [FromServices] SqlKataQueryRunner runner,
    [AsParameters] Query query)
    {
        var result = await runner.List<Query, Result>((qf) =>
        {
            var statement = qf.Query(Tables.Collections)
            .Select(Tables.Collections.AllFields)
            .Select(Tables.Clients.Field(nameof(Client.Name), nameof(Result.ClientName)))
            .Join(Tables.Clients, Tables.Collections.Field(nameof(Collection.ClientId)), Tables.Clients.Field(nameof(Client.ClientId)));

            if (!string.IsNullOrEmpty(query.Status))
            {
                statement = statement.Where(Tables.Collections.Field(nameof(Collection.Status)), query.Status);
            }
            if (query.ClientId.HasValue && query.ClientId != Guid.Empty)
            {
                statement = statement.Where(Tables.Collections.Field(nameof(Collection.ClientId)), query.ClientId);
            }
            return statement;
        }, query);

        return TypedResults.Ok(result);
    }

    public static async Task<RazorComponentResult> HandlePage(
    [AsParameters] Query query,
    [FromServices] ApplicationDbContext dbContext,
    [FromServices] SqlKataQueryRunner runner)
    {
        var clients = await dbContext.Set<Client>().AsNoTracking().ToListAsync();

        var result = await Handle(runner, query);

        return new RazorComponentResult<ListCollectionsPage>(new { Result = result.Value, Query = query, Clients = clients });
    }
}
