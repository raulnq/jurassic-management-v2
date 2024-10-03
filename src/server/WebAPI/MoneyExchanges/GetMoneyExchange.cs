using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;

namespace WebAPI.MoneyExchanges;

public static class GetMoneyExchange
{
    public class Query
    {
        public Guid MoneyExchangeId { get; set; }
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

    public static async Task<Ok<Result>> Handle(
    [FromServices] SqlKataQueryRunner runner,
    [FromRoute] Guid moneyExchangeId)
    {
        var result = await runner.Get<Result>((qf) => qf
                .Query(Tables.MoneyExchanges)
                .Where(Tables.MoneyExchanges.Field(nameof(MoneyExchange.MoneyExchangeId)), moneyExchangeId));

        return TypedResults.Ok(result);
    }
}
