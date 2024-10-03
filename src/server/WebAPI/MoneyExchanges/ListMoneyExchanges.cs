using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;

namespace WebAPI.MoneyExchanges;

public static class ListMoneyExchanges
{
    public class Query : ListQuery
    {
    }

    public class Result
    {
        public Guid MoneyExchangeId { get; set; }
        public decimal Rate { get; set; }
        public decimal ToAmount { get; set; }
        public decimal FromAmount { get; set; }
        public decimal ToITF { get; set; }
        public decimal FromITF { get; set; }
        public string? ToCurrency { get; set; }
        public string? FromCurrency { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTime? IssuedAt { get; set; }
    }

    public static async Task<Ok<ListResults<Result>>> Handle(
    [FromServices] SqlKataQueryRunner runner,
    [AsParameters] Query query)
    {
        var result = await runner.List<Query, Result>((qf) =>
        {
            var statement = qf.Query(Tables.MoneyExchanges);
            return statement;
        }, query);

        return TypedResults.Ok(result);
    }

    public static async Task<RazorComponentResult> HandlePage(
    [AsParameters] Query query,
    [FromServices] SqlKataQueryRunner runner)
    {
        var result = await Handle(runner, query);

        return new RazorComponentResult<ListMoneyExchangesPage>(new { Result = result.Value, Query = query });
    }
}
