using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Clients;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.InvoiceToCollectionProcesses;

namespace WebAPI.Collections;

public static class GetCollection
{
    public class Query
    {
        public Guid CollectionId { get; set; }
    }

    public class Result
    {
        public Guid CollectionId { get; set; }
        public Guid ClientId { get; set; }
        public string? ClientName { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public DateTimeOffset? CanceledAt { get; set; }
        public decimal Total { get; set; }
        public decimal Commission { get; set; }
        public string? Number { get; set; }
        public string? Status { get; set; }
        public decimal ITF { get; set; }
        public string? Currency { get; set; }
        public string? DocumentUrl { get; set; }
    }

    public static async Task<Ok<Result>> Handle(
    [FromServices] SqlKataQueryRunner runner,
    [FromRoute] Guid collectionId)
    {
        var result = await runner.Get<Result>((qf) => qf
                .Query(Tables.Collections)
                .Select(Tables.Collections.AllFields)
                .Select(Tables.Clients.Field(nameof(Client.Name), nameof(Result.ClientName)))
                .Join(Tables.Clients, Tables.Collections.Field(nameof(Collection.ClientId)), Tables.Clients.Field(nameof(Client.ClientId)))
                .Where(Tables.Collections.Field(nameof(Query.CollectionId)), collectionId));

        return TypedResults.Ok(result);
    }

    public static async Task<RazorComponentResult> HandlePage(
    [FromServices] SqlKataQueryRunner runner,
    [FromRoute] Guid collectionId)
    {
        var result = await Handle(runner, collectionId);

        var listInvoiceToCollectionProcessItemsQuery = new ListInvoiceToCollectionProcessItems.Query() { CollectionId = collectionId, PageSize = 5 };

        var listInvoiceToCollectionProcessItemsResult = await new ListInvoiceToCollectionProcessItems.Runner(runner).Run(listInvoiceToCollectionProcessItemsQuery);

        return new RazorComponentResult<GetCollectionPage>(new
        {
            Result = result.Value,
            ListInvoiceToCollectionProcessItemsResult = listInvoiceToCollectionProcessItemsResult,
            ListInvoiceToCollectionProcessItemsQuery = listInvoiceToCollectionProcessItemsQuery,
        });
    }
}
