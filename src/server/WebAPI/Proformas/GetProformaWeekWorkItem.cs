using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;

namespace WebAPI.Proformas;

public static class GetProformaWeekWorkItem
{
    public class Query
    {
        public Guid ProformaId { get; set; }
        public int Week { get; set; }
        public Guid CollaboratorId { get; set; }
    }

    public class Result
    {
        public Guid ProformaId { get; set; }
        public int Week { get; set; }
        public Guid CollaboratorId { get; set; }
        public Guid CollaboratorRoleId { get; set; }
        public decimal Hours { get; set; }
        public decimal FeeAmount { get; set; }
        public decimal FreeHours { get; set; }
        public decimal SubTotal { get; set; }
        public decimal ProfitAmount { get; set; }
        public decimal ProfitPercentage { get; set; }
    }

    public static async Task<Ok<Result>> Handle(
        [FromServices] SqlKataQueryRunner runner,
        [FromRoute] Guid proformaId,
        [FromRoute] int week,
        [FromRoute] Guid collaboratorId)
    {
        var result = await runner.Get<Result>((qf) => qf
                .Query(Tables.ProformaWeekWorkItems)
                .Where(Tables.ProformaWeekWorkItems.Field(nameof(ProformaWeekWorkItem.CollaboratorId)), collaboratorId)
                .Where(Tables.ProformaWeekWorkItems.Field(nameof(ProformaWeekWorkItem.Week)), week)
                .Where(Tables.ProformaWeekWorkItems.Field(nameof(ProformaWeekWorkItem.ProformaId)), proformaId));

        return TypedResults.Ok(result);
    }
}
