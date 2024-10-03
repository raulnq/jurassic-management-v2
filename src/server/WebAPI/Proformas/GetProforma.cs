using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Clients;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.ProformaDocuments;
using WebAPI.Projects;

namespace WebAPI.Proformas;

public static class GetProforma
{
    public class Query
    {
        public Guid ProformaId { get; set; }
    }

    public class Result
    {
        public Guid ProformaId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Number { get; set; } = default!;
        public Guid ProjectId { get; set; }
        public Guid ClientId { get; set; }
        public string? ProjectName { get; set; }
        public string? ClientName { get; set; }
        public string? ClientPhoneNumber { get; set; }
        public string? ClientDocumentNumber { get; set; }
        public string? ClientAddress { get; set; }
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
        public DateTime? IssuedAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? CanceledAt { get; set; }
        public string? Currency { get; set; }
        public string? Note { get; set; }
    }

    public class Runner : BaseRunner
    {
        public Runner(SqlKataQueryRunner queryRunner) : base(queryRunner) { }

        public Task<Result> Run(Query query)
        {
            return _queryRunner.Get<Result>((qf) => qf
                .Query(Tables.Proformas)
                .Select(Tables.Proformas.AllFields)
                .Select(Tables.Projects.Field(nameof(Project.Name), nameof(Result.ProjectName)))
                .Select(
                    Tables.Clients.Field(nameof(Client.ClientId), nameof(Result.ClientId)),
                    Tables.Clients.Field(nameof(Client.Name), nameof(Result.ClientName)),
                    Tables.Clients.Field(nameof(Client.PhoneNumber), nameof(Result.ClientPhoneNumber)),
                    Tables.Clients.Field(nameof(Client.Address), nameof(Result.ClientAddress)),
                    Tables.Clients.Field(nameof(Client.DocumentNumber), nameof(Result.ClientDocumentNumber))
                )
                .Join(Tables.Projects, Tables.Projects.Field(nameof(Project.ProjectId)), Tables.Proformas.Field(nameof(Proforma.ProjectId)))
                .Join(Tables.Clients, Tables.Clients.Field(nameof(Client.ClientId)), Tables.Projects.Field(nameof(Project.ClientId)))
                .Where(Tables.Proformas.Field(nameof(Proforma.ProformaId)), query.ProformaId));
        }
    }

    public static async Task<Ok<Result>> Handle(
    [FromServices] SqlKataQueryRunner runner,
    [FromRoute] Guid proformaId)
    {
        return TypedResults.Ok(await new Runner(runner).Run(new Query() { ProformaId = proformaId }));
    }

    public static async Task<RazorComponentResult> HandlePage(
        [FromServices] SqlKataQueryRunner runner,
        [FromServices] ApplicationDbContext dbContext,
        [FromRoute] Guid proformaId)
    {
        var getProformaResult = await Handle(runner, proformaId);

        var listProformaWeeksQuery = new ListProformaWeeks.Query() { ProformaId = proformaId, PageSize = 5 };

        var listProformaWeeksResult = await new ListProformaWeeks.Runner(runner).Run(listProformaWeeksQuery);

        var proformaDocument = await dbContext.Set<ProformaDocument>().AsNoTracking().FirstOrDefaultAsync(d => d.ProformaId == proformaId);

        return new RazorComponentResult<GetProformaPage>(new
        {
            GetProformaResult = getProformaResult.Value,
            ListProformaWeeksResult = listProformaWeeksResult,
            ListProformaWeeksQuery = listProformaWeeksQuery,
            ProformaDocument = proformaDocument,
        });
    }
}
