using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;

namespace WebAPI.Transactions;

public static class ListTransactions
{
    public class Query : ListQuery
    {
        public string? Type { get; set; }
    }

    public class Result
    {
        public Guid TransactionId { get; set; }
        public string? Description { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Taxes { get; set; }
        public decimal Total { get; set; }
        public decimal ITF { get; set; }
        public string? Number { get; set; }
        public string? DocumentUrl { get; set; }
        public string? Currency { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTime IssuedAt { get; set; }
        public string Type { get; set; } = default!;
    }

    public static async Task<Ok<ListResults<Result>>> Handle(
    [FromServices] SqlKataQueryRunner runner,
    [AsParameters] Query query)
    {
        var result = await runner.List<Query, Result>((qf) =>
        {
            var statement = qf.Query(Tables.Transactions);

            if (!string.IsNullOrEmpty(query.Type))
            {
                statement = statement.WhereLike(Tables.Transactions.Field(nameof(Transaction.Type)), query.Type);
            }
            return statement;
        }, query);

        return TypedResults.Ok(result);
    }

    public static async Task<RazorComponentResult> HandlePage(
    [AsParameters] Query query,
    [FromServices] SqlKataQueryRunner runner)
    {
        var result = await Handle(runner, query);

        return new RazorComponentResult<ListTransactionsPage>(new { Result = result.Value, Query = query });
    }
}
