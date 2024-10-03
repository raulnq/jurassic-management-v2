using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;

namespace WebAPI.InvoiceToCollectionProcesses;

public static class ListInvoiceToCollectionProcessItems
{
    public class Query : ListQuery
    {
        public Guid? CollectionId { get; set; }
    }

    public class Result
    {
        public Guid CollectionId { get; set; }
        public Guid InvoiceId { get; set; }
        public string? InvoiceNumber { get; set; }
        public string? InvoiceCurreny { get; set; }
        public decimal InvoiceSubTotal { get; set; }
        public decimal InvoiceTaxes { get; set; }
        public decimal InvoiceTotal { get; set; }
    }

    public class Runner : BaseRunner
    {
        public Runner(SqlKataQueryRunner queryRunner) : base(queryRunner) { }

        public Task<ListResults<Result>> Run(Query query)
        {
            return _queryRunner.List<Query, Result>((qf) => qf.Query(Tables.VwInvoiceToCollectionProcessItems)
                .Where(Tables.VwInvoiceToCollectionProcessItems.Field(nameof(InvoiceToCollectionProcessItem.CollectionId)), query.CollectionId), query);
        }
    }

    public static async Task<RazorComponentResult> HandlePage(
    [AsParameters] Query query,
    [FromServices] SqlKataQueryRunner runner)
    {
        var result = await new Runner(runner).Run(query);
        return new RazorComponentResult<ListInvoiceToCollectionProcessItemsPage>(new { Result = result, Query = query });
    }
}
