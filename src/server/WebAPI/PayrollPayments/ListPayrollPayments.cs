using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Collaborators;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.MoneyExchanges;

namespace WebAPI.PayrollPayments;

public static class ListPayrollPayments
{
    public class Query : ListQuery
    {
        public string? Status { get; set; }
    }

    public class Result
    {
        public Guid PayrollPaymentId { get; set; }
        public Guid CollaboratorId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public decimal GrossSalary { get; set; }
        public decimal NetSalary { get; set; }
        public decimal Afp { get; set; }
        public decimal Commission { get; set; }
        public string? Status { get; set; }
        public string? CollaboratorName { get; set; }
        public string? DocumentUrl { get; set; }
        public decimal ITF { get; set; }
        public string? Currency { get; set; }
        public DateTimeOffset? CanceledAt { get; set; }
        public DateTime? AfpPaidAt { get; set; }
        public Guid? MoneyExchangeId { get; set; }
        public decimal Rate { get; set; }
        public decimal GrossSalaryInOriginalCurrency { get { return Rate != 0 ? GrossSalary / Rate : 0; } }
    }

    public static async Task<Ok<ListResults<Result>>> Handle(
    [FromServices] SqlKataQueryRunner runner,
    [AsParameters] Query query)
    {
        var result = await runner.List<Query, Result>((qf) =>
        {
            var statement = qf.Query(Tables.PayrollPayments)
            .Select(Tables.PayrollPayments.AllFields)
            .Select(Tables.MoneyExchanges.Field(nameof(MoneyExchange.Rate)))
            .Select(Tables.Collaborators.Field(nameof(Collaborator.Name), nameof(Result.CollaboratorName)))
            .Join(Tables.Collaborators, Tables.PayrollPayments.Field(nameof(PayrollPayment.CollaboratorId)), Tables.Collaborators.Field(nameof(Collaborator.CollaboratorId)))
            .LeftJoin(Tables.MoneyExchanges, Tables.PayrollPayments.Field(nameof(PayrollPayment.MoneyExchangeId)), Tables.MoneyExchanges.Field(nameof(MoneyExchange.MoneyExchangeId)));
            if (!string.IsNullOrEmpty(query.Status))
            {
                statement = statement.Where(Tables.PayrollPayments.Field(nameof(PayrollPayment.Status)), query.Status);
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
        return new RazorComponentResult<ListPayrollPaymentsPage>(new { Result = result.Value, Query = query });
    }
}
