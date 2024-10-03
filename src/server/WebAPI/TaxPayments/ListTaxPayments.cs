using Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;

namespace WebAPI.TaxPayments;

public static class ListTaxPayments
{
    public class Query : ListQuery
    {
        public string? Month { get; set; }

        public int? Year { get; set; }
    }

    public class Result
    {
        public Guid TaxPaymentId { get; set; }
        public decimal Total { get; set; }
        public decimal ITF { get; set; }
        public string? Month { get; set; }
        public int Year { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? PaidAt { get; set; }
        public string? DocumentUrl { get; set; }
        public string? Status { get; set; }
    }

    public static async Task<Ok<ListResults<Result>>> Handle(
    [FromServices] SqlKataQueryRunner runner,
    [AsParameters] Query query)
    {
        var result = await runner.List<Query, Result>((qf) =>
        {
            var statement = qf.Query(Tables.TaxPayments);

            if (!string.IsNullOrEmpty(query.Month))
            {
                statement = statement.Where(Tables.TaxPayments.Field(nameof(TaxPayment.Month)), query.Month);
            }
            statement = statement.Where(Tables.TaxPayments.Field(nameof(TaxPayment.Year)), query.Year);
            return statement;
        }, query);

        return TypedResults.Ok(result);
    }

    public static async Task<RazorComponentResult> HandlePage(
    [AsParameters] Query query,
    [FromServices] IClock clock,
    [FromServices] SqlKataQueryRunner runner)
    {
        if (!query.Year.HasValue)
        {
            query.Year = clock.Now.Year;
        }

        var result = await Handle(runner, query);

        return new RazorComponentResult<ListTaxPaymentsPage>(new { Result = result.Value, Query = query });
    }
}
