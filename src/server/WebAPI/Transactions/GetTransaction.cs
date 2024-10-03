using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;

namespace WebAPI.Transactions;

public static class GetTransaction
{
    public class Query
    {
        public Guid TransactionId { get; set; }
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

    public static async Task<Ok<Result>> Handle(
    [FromServices] SqlKataQueryRunner runner,
    [FromRoute] Guid transactionId)
    {
        var result = await runner.Get<Result>((qf) => qf
                .Query(Tables.Transactions)
                .Where(Tables.Transactions.Field(nameof(Transaction.TransactionId)), transactionId));

        return TypedResults.Ok(result);
    }
}
