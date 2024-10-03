using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;

namespace WebAPI.ProformaToCollaboratorPaymentProcesses;

public static class ListProformaToCollaboratorPaymentProcessItems
{
    public class Query : ListQuery
    {
        public Guid? CollaboratorPaymentId { get; set; }
    }

    public class Result
    {
        public Guid CollaboratorPaymentId { get; set; }
        public Guid ProformaId { get; set; }
        public int Week { get; set; }
        public Guid CollaboratorId { get; set; }
        public string? CollaboratorName { get; set; }
        public string? CollaboratorEmail { get; set; }
        public string? ProformaNote { get; set; }

        public string? ProjectName { get; set; }
        public string? ProformaNumber { get; set; }
        public string? ProformaCurrency { get; set; }
        public DateTime ProformaWeekStart { get; set; }
        public DateTime ProformaWeekEnd { get; set; }
        public decimal Hours { get; set; }
        public decimal FreeHours { get; set; }
        public decimal FeeAmount { get; set; }
        public decimal SubTotal { get; set; }
        public decimal ProfitAmount { get; set; }
        public decimal ProfitPercentage { get; set; }

    }

    public class Runner : BaseRunner
    {
        public Runner(SqlKataQueryRunner queryRunner) : base(queryRunner) { }

        public Task<ListResults<Result>> Run(Query query)
        {
            return _queryRunner.List<Query, Result>((qf) => qf.Query(Tables.VwProformaToCollaboratorPaymentProcessItems)
                .Where(Tables.VwProformaToCollaboratorPaymentProcessItems.Field(nameof(ProformaToCollaboratorPaymentProcessItem.CollaboratorPaymentId)), query.CollaboratorPaymentId), query);
        }
    }

    public static async Task<RazorComponentResult> HandlePage(
    [AsParameters] Query query,
    [FromRoute] Guid collaboratoPaymentId,
    [FromServices] SqlKataQueryRunner runner)
    {
        query.CollaboratorPaymentId = collaboratoPaymentId;
        var result = await new Runner(runner).Run(query);
        return new RazorComponentResult<ListProformaToCollaboratorPaymentProcessItemsPage>(new { Result = result, Query = query });
    }
}
