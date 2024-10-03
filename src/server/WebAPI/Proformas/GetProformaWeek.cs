using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;

namespace WebAPI.Proformas;

public static class GetProformaWeek
{
    public class Query
    {
        public Guid ProformaId { get; set; }
        public int Week { get; set; }
    }

    public class Result
    {
        public Guid ProformaId { get; set; }
        public int Week { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public decimal Penalty { get; set; }
        public decimal SubTotal { get; set; }
    }

    public static async Task<Ok<Result>> Handle(
    [FromServices] SqlKataQueryRunner runner,
    [FromRoute] Guid proformaId,
    [FromRoute] int week)
    {
        var result = await runner.Get<Result>((qf) => qf
                .Query(Tables.ProformaWeeks)
                .Where(Tables.ProformaWeeks.Field(nameof(ProformaWeek.Week)), week)
                .Where(Tables.ProformaWeeks.Field(nameof(ProformaWeek.ProformaId)), proformaId));

        return TypedResults.Ok(result);
    }
}
