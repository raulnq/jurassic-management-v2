using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;

namespace WebAPI.BankBalance;

public static class ListBankBalance
{
    public class Query : ListQuery
    {
        public string? StringStart { get; set; }
        public string? StringEnd { get; set; }
        [JsonIgnore]
        public DateTime? Start { get; set; }
        [JsonIgnore]
        public DateTime? End { get; set; }
        public string? Currency { get; set; }
    }

    public class Result
    {
        public Guid RecordId { get; set; }
        public DateTime IssuedAt { get; set; }
        public string Type { get; set; } = default!;
        public string? Description { get; set; }
        public string Currency { get; set; } = default!;
        public decimal SubTotal { get; set; }
        public decimal Taxes { get; set; }
        public decimal Total { get; set; }
        public decimal ITF { get; set; }
        public decimal Balance { get; set; }
        public string? Number { get; set; }
        public string Source { get; set; } = default!;
        public DateTimeOffset CreatedAt { get; set; }
        public int Sign { get; set; }
        public decimal SignedTotal
        {
            get
            {
                return Total * Sign - ITF;
            }
        }
    }

    public static async Task<RazorComponentResult> HandlePage(
    [AsParameters] Query query,
    [FromServices] SqlKataQueryRunner runner)
    {
        var result = new List<Result>();

        if (!string.IsNullOrEmpty(query.StringStart))
        {
            if (DateTime.TryParse(query.StringStart, out DateTime dts))
            {
                query.Start = dts;
            }
        }

        if (!string.IsNullOrEmpty(query.StringEnd))
        {
            if (DateTime.TryParse(query.StringEnd, out DateTime dts))
            {
                query.End = dts;
            }
        }

        if (!string.IsNullOrEmpty(query.Currency))
        {
            result = await runner.List<Result>(qf =>
            {
                var statement = qf
                .Query(Tables.VwBankBalance)
                .OrderBy(Tables.VwBankBalance.Field("IssuedAt"))
                .Where(Tables.VwBankBalance.Field("Currency"), query.Currency);

                if (query.End.HasValue && query.Start.HasValue)
                {
                    statement = statement.WhereBetween(Tables.VwBankBalance.Field("IssuedAt"), query.Start, query.End);
                }
                else
                {
                    if (query.Start.HasValue)
                    {
                        statement = statement.Where(Tables.VwBankBalance.Field("IssuedAt"), ">=", query.Start);
                    }

                    if (query.End.HasValue)
                    {
                        statement = statement.Where(Tables.VwBankBalance.Field("IssuedAt"), "<=", query.End);
                    }
                }

                return statement;
            });
        }

        var startBalance = 0m;

        if (query.Start.HasValue && !string.IsNullOrEmpty(query.Currency))
        {
            startBalance = (await new GetBankBalance.Runner(runner).Run(new GetBankBalance.Query() { Currency = query.Currency, End = query.Start.Value.AddDays(-1) })).Total;
        }

        var endBalance = startBalance;

        foreach (var item in result)
        {
            item.Balance = item.SignedTotal + endBalance;
            endBalance = item.SignedTotal + endBalance;
        }

        return new RazorComponentResult<ListBankBalancePage>(new { Result = result, Query = query, StartBalance = startBalance, EndBalance = endBalance });
    }
}
