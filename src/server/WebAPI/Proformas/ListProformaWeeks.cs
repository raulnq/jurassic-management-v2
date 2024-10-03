using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;

namespace WebAPI.Proformas;

public static class ListProformaWeeks
{
    public class Query : ListQuery
    {
        public Guid? ProformaId { get; set; }
    }

    public class Result
    {
        public Guid ProformaId { get; set; }
        public int Week { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public decimal Penalty { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Hours { get; set; }
    }

    public class Runner : BaseRunner
    {
        public Runner(SqlKataQueryRunner queryRunner) : base(queryRunner) { }

        public Task<ListResults<Result>> Run(Query query)
        {
            return _queryRunner.List<Query, Result>((qf) =>
                qf
                .Query(Tables.ProformaWeeks)
                .Select(
                    Tables.ProformaWeeks.Field(nameof(ProformaWeek.ProformaId)),
                    Tables.ProformaWeeks.Field(nameof(ProformaWeek.Week)),
                    Tables.ProformaWeeks.Field(nameof(ProformaWeek.Start)),
                    Tables.ProformaWeeks.Field(nameof(ProformaWeek.End)),
                    Tables.ProformaWeeks.Field(nameof(ProformaWeek.Penalty)),
                    Tables.ProformaWeeks.Field(nameof(ProformaWeek.SubTotal))
                    )
                .SelectRaw($"sum({Tables.ProformaWeekWorkItems.Field(nameof(ProformaWeekWorkItem.Hours))}) as Hours")
                .LeftJoin(Tables.ProformaWeekWorkItems, x => x
                    .On(Tables.ProformaWeekWorkItems.Field(nameof(ProformaWeekWorkItem.ProformaId)), Tables.ProformaWeeks.Field(nameof(ProformaWeek.ProformaId)))
                    .On(Tables.ProformaWeekWorkItems.Field(nameof(ProformaWeekWorkItem.Week)), Tables.ProformaWeeks.Field(nameof(ProformaWeek.Week)))
                )
                .GroupBy(
                    Tables.ProformaWeeks.Field(nameof(ProformaWeek.ProformaId)),
                    Tables.ProformaWeeks.Field(nameof(ProformaWeek.Week)),
                    Tables.ProformaWeeks.Field(nameof(ProformaWeek.Start)),
                    Tables.ProformaWeeks.Field(nameof(ProformaWeek.End)),
                    Tables.ProformaWeeks.Field(nameof(ProformaWeek.Penalty)),
                    Tables.ProformaWeeks.Field(nameof(ProformaWeek.SubTotal))
                    )
                .Where(Tables.ProformaWeeks.Field(nameof(ProformaWeek.ProformaId)), query.ProformaId), query);
        }
    }

    public static async Task<Ok<ListResults<Result>>> Handle(
        [FromServices] SqlKataQueryRunner runner,
        [FromRoute] Guid proformaId,
        [AsParameters] Query query)
    {
        query.ProformaId = proformaId;
        return TypedResults.Ok(await new Runner(runner).Run(query));
    }

    public static async Task<RazorComponentResult> HandlePage(
        [AsParameters] Query query,
        [FromRoute] Guid proformaId,
        [FromServices] SqlKataQueryRunner runner)
    {
        var result = await Handle(runner, proformaId, query);
        return new RazorComponentResult<ListProformaWeeksPage>(new { Result = result.Value, Query = query });
    }
}
