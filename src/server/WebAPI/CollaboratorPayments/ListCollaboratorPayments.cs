using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Collaborators;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;

namespace WebAPI.CollaboratorPayments;

public static class ListCollaboratorPayments
{
    public class Query : ListQuery
    {
        public string? Status { get; set; }
    }

    public class Result
    {
        public Guid CollaboratorPaymentId { get; set; }
        public Guid CollaboratorId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public decimal GrossSalary { get; set; }
        public decimal NetSalary { get; set; }
        public decimal Withholding { get; set; }
        public string? Status { get; set; }
        public string? DocumentUrl { get; set; }
        public string? Number { get; set; }
        public string? CollaboratorName { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public decimal ITF { get; set; }
        public string? Currency { get; set; }
        public DateTimeOffset? CanceledAt { get; set; }
    }

    public static async Task<Ok<ListResults<Result>>> Handle(
    [FromServices] SqlKataQueryRunner runner,
    [AsParameters] Query query)
    {
        var result = await runner.List<Query, Result>((qf) =>
        {
            var statement = qf.Query(Tables.CollaboratorPayments)
            .Select(Tables.CollaboratorPayments.AllFields)
            .Select(Tables.Collaborators.Field(nameof(Collaborator.Name), nameof(Result.CollaboratorName)))
            .Join(Tables.Collaborators, Tables.CollaboratorPayments.Field(nameof(CollaboratorPayment.CollaboratorId)), Tables.Collaborators.Field(nameof(Collaborator.CollaboratorId)));

            if (!string.IsNullOrEmpty(query.Status))
            {
                statement = statement.Where(Tables.CollaboratorPayments.Field(nameof(CollaboratorPayment.Status)), query.Status);
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
        return new RazorComponentResult<ListCollaboratorPaymentsPage>(new { Result = result.Value, Query = query });
    }
}
