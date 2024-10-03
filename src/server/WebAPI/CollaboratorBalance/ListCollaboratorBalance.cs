using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using WebAPI.Collaborators;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;

namespace WebAPI.CollaboratorBalance;

public static class ListCollaboratorBalance
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
        public Guid? CollaboratorId { get; set; }
    }

    public class Result
    {
        public Guid CollaboratorId { get; set; }
        public DateTime Date { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public string Name { get; set; } = default!;
        public string Currency { get; set; } = default!;
        public decimal NetSalary { get; set; }
        public decimal GrossSalary { get; set; }
        public decimal Withholding { get; set; }
        public decimal Balance { get; set; }
        public string? Number { get; set; }
        public int Sign { get; set; }
        public decimal SignedNetSalary
        {
            get
            {
                return NetSalary * Sign;
            }
        }
        public decimal SignedGrossSalary
        {
            get
            {
                return GrossSalary * Sign;
            }
        }

        public decimal SignedWithholding
        {
            get
            {
                return Withholding * Sign;
            }
        }
    }

    public static async Task<RazorComponentResult> HandlePage(
    [AsParameters] Query query,
    [FromServices] ApplicationDbContext dbContext,
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

        if (!string.IsNullOrEmpty(query.Currency) && query.CollaboratorId != Guid.Empty)
        {
            result = await runner.List<Result>(qf =>
            {
                var statement = qf
                .Query(Tables.VwCollaboratorBalance)
                .OrderBy(Tables.VwCollaboratorBalance.Field("Date"))
                .Where(Tables.VwCollaboratorBalance.Field("Currency"), query.Currency)
                .Where(Tables.VwCollaboratorBalance.Field("CollaboratorId"), query.CollaboratorId);
                if (query.End.HasValue && query.Start.HasValue)
                {
                    statement = statement.WhereBetween(Tables.VwCollaboratorBalance.Field("Date"), query.Start, query.End);
                }
                else
                {
                    if (query.Start.HasValue)
                    {
                        statement = statement.Where(Tables.VwCollaboratorBalance.Field("Date"), ">=", query.Start);
                    }

                    if (query.End.HasValue)
                    {
                        statement = statement.Where(Tables.VwCollaboratorBalance.Field("Date"), "<=", query.End);
                    }
                }

                return statement;
            });
        }

        var startBalance = 0m;

        if (query.Start.HasValue && !string.IsNullOrEmpty(query.Currency))
        {
            startBalance = (await new GetCollaboratorBalance.Runner(runner).Run(new GetCollaboratorBalance.Query() { Currency = query.Currency, End = query.Start.Value.AddDays(-1) })).Total;
        }

        var endBalance = startBalance;

        foreach (var item in result)
        {
            item.Balance = item.SignedNetSalary + endBalance;
            endBalance = item.SignedNetSalary + endBalance;
        }

        var collaborators = await dbContext.Set<Collaborator>().AsNoTracking().ToListAsync();

        return new RazorComponentResult<ListCollaboratorBalancePage>(new
        {
            Result = result,
            Query = query,
            StartBalance = startBalance,
            EndBalance = endBalance,
            Collaborators = collaborators
        });
    }
}
