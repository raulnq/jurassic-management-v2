using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;

namespace WebAPI.ProformaToInvoiceProcesses;

public static class ListProformaToInvoiceProcessItems
{
    public class Query : ListQuery
    {
        public Guid? InvoiceId { get; set; }
    }

    public class Result
    {
        public Guid ProformaId { get; set; }
        public Guid InvoiceId { get; set; }
        public string? ProjectName { get; set; }
        public DateTime ProformaStart { get; set; }
        public DateTime ProformaEnd { get; set; }
        public string? ProformaNumber { get; set; }
        public string? ProformaCurrency { get; set; }
        public decimal ProformaCommission { get; set; }
        public decimal ProformaSubTotal { get; set; }
        public decimal ProformaTotal { get; set; }
    }

    public class Runner : BaseRunner
    {
        public Runner(SqlKataQueryRunner queryRunner) : base(queryRunner) { }

        public Task<ListResults<Result>> Run(Query query)
        {
            return _queryRunner.List<Query, Result>((qf) => qf.Query(Tables.VwProformaToInvoiceProcessItems)
                .Where(Tables.VwProformaToInvoiceProcessItems.Field(nameof(ProformaToInvoiceProcessItem.InvoiceId)), query.InvoiceId), query);
        }
    }

    public static async Task<RazorComponentResult> HandlePage(
    [AsParameters] Query query,
    [FromServices] SqlKataQueryRunner runner)
    {
        var result = await new Runner(runner).Run(query);
        return new RazorComponentResult<ListProformaToInvoiceProcessItemsPage>(new { Result = result, Query = query });
    }
}
