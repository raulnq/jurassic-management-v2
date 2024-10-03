using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.CollaboratorRoles;
using WebAPI.Collaborators;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.JiraProfiles;

namespace WebAPI.Proformas;

public static class ListProformaWeekWorkItems
{
    public class Query : ListQuery
    {
        public Guid? ProformaId { get; set; }
        public Guid? CollaboratorId { get; set; }
        public int? Week { get; set; }
        public IEnumerable<Guid>? ListOfProformaId { get; set; }
    }

    public class Result
    {
        public Guid ProformaId { get; set; }
        public int Week { get; set; }
        public Guid CollaboratorId { get; set; }
        public string? CollaboratorName { get; set; }
        public Guid CollaboratorRoleId { get; set; }
        public string? CollaboratorRoleName { get; set; }
        public decimal Hours { get; set; }
        public decimal FeeAmount { get; set; }
        public decimal FreeHours { get; set; }
        public decimal SubTotal { get; set; }
        public decimal ProfitAmount { get; set; }
        public decimal ProfitPercentage { get; set; }
        public decimal WithholdingPercentage { get; set; }
        public decimal Withholding { get; set; }
        public decimal GrossSalary { get; set; }
        public decimal NetSalary { get; set; }
        public string? Status { get; set; }
        public string Currency { get; set; } = default!;
    }

    public class Runner : BaseRunner
    {
        public Runner(SqlKataQueryRunner queryRunner) : base(queryRunner) { }

        public Task<ListResults<Result>> Run(Query query)
        {
            return _queryRunner.List<Query, Result>((qf) =>
            {
                var statement = qf.Query(Tables.ProformaWeekWorkItems)
                .Select(Tables.ProformaWeekWorkItems.AllFields)
                .Select(Tables.Collaborators.Field(nameof(Collaborator.Name), nameof(Result.CollaboratorName)))
                .Select(Tables.CollaboratorRoles.Field(nameof(CollaboratorRole.Name), nameof(Result.CollaboratorRoleName)))
                .Select(Tables.Proformas.Field(nameof(Proforma.Status)), Tables.Proformas.Field(nameof(Proforma.Currency)))
                .Join(Tables.Collaborators, Tables.ProformaWeekWorkItems.Field(nameof(ProformaWeekWorkItem.CollaboratorId)), Tables.Collaborators.Field(nameof(Collaborator.CollaboratorId)))
                .Join(Tables.CollaboratorRoles, Tables.ProformaWeekWorkItems.Field(nameof(ProformaWeekWorkItem.CollaboratorRoleId)), Tables.CollaboratorRoles.Field(nameof(CollaboratorRole.CollaboratorRoleId)))
                .Join(Tables.Proformas, Tables.Proformas.Field(nameof(Proforma.ProformaId)), Tables.ProformaWeekWorkItems.Field(nameof(ProformaWeekWorkItem.ProformaId)));

                if (query.ProformaId.HasValue)
                {
                    statement = statement
                        .Where(Tables.ProformaWeekWorkItems.Field(nameof(ProformaWeekWorkItem.ProformaId)), query.ProformaId);
                }

                if (query.CollaboratorId.HasValue)
                {
                    statement = statement
                        .Where(Tables.ProformaWeekWorkItems.Field(nameof(ProformaWeekWorkItem.CollaboratorId)), query.CollaboratorId);
                }

                if (query.Week.HasValue)
                {
                    statement = statement
                        .Where(Tables.ProformaWeekWorkItems.Field(nameof(ProformaWeekWorkItem.Week)), query.Week);
                }

                if (query.ListOfProformaId != null && query.ListOfProformaId.Any())
                {
                    statement = statement
                        .WhereIn(Tables.ProformaWeekWorkItems.Field(nameof(ProformaWeekWorkItem.ProformaId)), query.ListOfProformaId);
                }

                return statement;
            }, query);
        }
    }

    public static async Task<Ok<ListResults<Result>>> Handle(
    [FromServices] SqlKataQueryRunner runner,
    [FromRoute] Guid proformaId,
    [FromRoute] int week,
    [AsParameters] Query query)
    {
        query.ProformaId = proformaId;
        query.Week = week;
        return TypedResults.Ok(await new Runner(runner).Run(query));
    }

    public static async Task<RazorComponentResult> HandlePage(
        [AsParameters] Query query,
        [FromServices] SqlKataQueryRunner runner,
        [FromServices] ApplicationDbContext dbContext,
        [FromRoute] Guid proformaId,
        [FromRoute] int week)
    {
        var result = await Handle(runner, proformaId, week, query);

        var proforma = await dbContext.Set<Proforma>().AsNoTracking().FirstAsync(p => p.ProformaId == proformaId);

        var proformaWeek = await dbContext.Set<ProformaWeek>().AsNoTracking().Include(pw => pw.WorkItems).FirstAsync(pw => pw.ProformaId == proformaId && pw.Week == week);

        var anyJiraProfileProject = await dbContext.Set<JiraProfileProject>().AsNoTracking().AnyAsync(j => j.ProjectId == proforma.ProjectId);

        return new RazorComponentResult<ListProformaWeekWorkItemsPage>(new
        {
            Result = result.Value,
            Query = query,
            Proforma = proforma,
            ProformaWeek = proformaWeek,
            AnyJiraProfileProject = anyJiraProfileProject
        });
    }
}
