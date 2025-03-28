using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Clients;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Projects;

namespace WebAPI.Proformas;

public static class ListProformas
{
    public class Query : ListQuery
    {
        public string? Status { get; set; }
        public string? Number { get; set; }
        public IEnumerable<Guid>? ProformaId { get; set; }
        public string? Month { get; set; }
        public Guid? ClientId { get; set; }
    }

    public class Result
    {
        public Guid ProformaId { get; set; }
        public string? ProjectName { get; set; }
        public string? ClientName { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Number { get; set; } = default!;
        public Guid ProjectId { get; set; }
        public decimal Total { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Commission { get; set; }
        public decimal Discount { get; set; }
        public decimal MinimumHours { get; set; }
        public decimal PenaltyMinimumHours { get; set; }
        public decimal TaxesExpensesPercentage { get; set; }
        public decimal AdministrativeExpensesPercentage { get; set; }
        public decimal BankingExpensesPercentage { get; set; }
        public decimal MinimumBankingExpenses { get; set; }
        public decimal TaxesExpensesAmount { get; set; }
        public decimal AdministrativeExpensesAmount { get; set; }
        public decimal BankingExpensesAmount { get; set; }
        public string? Status { get; set; }
        public DateTimeOffset? IssuedAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string Currency { get; set; } = default!;
        public DateTimeOffset? CanceledAt { get; set; }
    }

    public static async Task<Ok<ListResults<Result>>> Handle(
        [FromServices] SqlKataQueryRunner runner,
        [AsParameters] Query query)
    {
        var result = await runner.List<Query, Result>((qf) =>
        {
            var statement = qf.Query(Tables.Proformas)
            .Select(Tables.Proformas.AllFields)
            .Select(Tables.Projects.Field(nameof(Project.Name), nameof(Result.ProjectName)))
            .Select(Tables.Clients.Field(nameof(Client.Name), nameof(Result.ClientName)))
            .Join(Tables.Projects, Tables.Projects.Field(nameof(Project.ProjectId)), Tables.Proformas.Field(nameof(Proforma.ProjectId)))
            .Join(Tables.Clients, Tables.Clients.Field(nameof(Client.ClientId)), Tables.Projects.Field(nameof(Project.ClientId)))
            .OrderByDesc(Tables.Proformas.Field(nameof(Proforma.Start)))
            ;

            if (!string.IsNullOrEmpty(query.Status))
            {
                statement = statement.Where(Tables.Proformas.Field(nameof(Proforma.Status)), query.Status);
            }
            if (!string.IsNullOrEmpty(query.Number))
            {
                statement = statement.WhereLike(Tables.Proformas.Field(nameof(Proforma.Number)), $"%{query.Number}%");
            }
            if (query.ProformaId != null && query.ProformaId.Any())
            {
                statement = statement.WhereIn(Tables.Proformas.Field(nameof(Proforma.ProformaId)), query.ProformaId);
            }
            if (!string.IsNullOrEmpty(query.Month))
            {
                // Month input value format is YYYY-MM
                var parts = query.Month.Split('-');
                if (parts.Length == 2 && int.TryParse(parts[0], out int year) && int.TryParse(parts[1], out int month))
                {
                    statement = statement.WhereRaw($"YEAR({Tables.Proformas.Field(nameof(Proforma.Start))}) = ? AND MONTH({Tables.Proformas.Field(nameof(Proforma.Start))}) = ?", year, month);
                }
            }
            if (query.ClientId.HasValue && query.ClientId != Guid.Empty)
            {
                statement = statement.Where(Tables.Clients.Field(nameof(Client.ClientId)), query.ClientId.Value);
            }
            return statement;
        }, query);

        return TypedResults.Ok(result);
    }

    public static async Task<RazorComponentResult> HandlePage(
        [AsParameters] Query query,
        [FromServices] SqlKataQueryRunner runner,
        [FromServices] ApplicationDbContext dbContext)
    {
        var result = await Handle(runner, query);

        var clients = await dbContext.Set<Client>().AsNoTracking().ToListAsync();

        return new RazorComponentResult<ListProformasPage>(new
        {
            Result = result.Value,
            Query = query,
            Clients = clients
        });
    }
}
