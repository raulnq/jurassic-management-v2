using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;

namespace WebAPI.Clients;

public static class ListClients
{
    public class Query : ListQuery
    {
        public string? Name { get; set; }
    }

    public class Result
    {
        public Guid ClientId { get; set; }
        public string Name { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public string DocumentNumber { get; set; } = default!;
        public string Address { get; set; } = default!;
        public decimal PenaltyMinimumHours { get; set; }
        public decimal PenaltyAmount { get; set; }
        public decimal TaxesExpensesPercentage { get; set; }
        public decimal AdministrativeExpensesPercentage { get; set; }
        public decimal BankingExpensesPercentage { get; set; }
        public decimal MinimumBankingExpenses { get; set; }
    }

    public static async Task<Ok<ListResults<Result>>> Handle(
    [FromServices] SqlKataQueryRunner runner,
    [AsParameters] Query query)
    {
        var result = await runner.List<Query, Result>((qf) =>
        {
            var statement = qf.Query(Tables.Clients);

            if (!string.IsNullOrEmpty(query.Name))
            {
                statement = statement.WhereLike(Tables.Clients.Field(nameof(Client.Name)), $"%{query.Name}%");
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

        return new RazorComponentResult<ListClientsPage>(new { Result = result.Value, Query = query });
    }
}
