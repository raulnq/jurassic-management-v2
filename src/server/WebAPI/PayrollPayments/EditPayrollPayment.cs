using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Infrastructure.Ui;
using WebAPI.MoneyExchanges;
using WebAPI.Proformas;

namespace WebAPI.PayrollPayments;

public static class EditPayrollPayment
{
    public class Command
    {
        public decimal NetSalary { get; set; }
        public decimal Afp { get; set; }
        public decimal Commission { get; set; }
        public Currency Currency { get; set; }
        public Guid MoneyExchangeId { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.NetSalary).GreaterThan(0);
            RuleFor(command => command.Afp).GreaterThanOrEqualTo(0);
            RuleFor(command => command.Commission).GreaterThanOrEqualTo(0);
        }
    }

    public static async Task<Ok> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromRoute] Guid payrollPaymentId,
    [FromServices] ApplicationDbContext dbContext,
    [FromBody] Command command)
    {
        new Validator().ValidateAndThrow(command);

        await behavior.Handle(async () =>
        {
            var payment = await dbContext.Get<PayrollPayment>(payrollPaymentId);

            payment.Edit(command.NetSalary, command.Currency, command.Afp, command.Commission, command.MoneyExchangeId);
        });

        return TypedResults.Ok();
    }

    public static async Task<RazorComponentResult> HandleAction(
    [FromServices] TransactionBehavior behavior,
    [FromServices] SqlKataQueryRunner runner,
    [FromBody] Command command,
    [FromServices] ApplicationDbContext dbContext,
    Guid payrollPaymentId,
    HttpContext context)
    {
        await Handle(behavior, payrollPaymentId, dbContext, command);

        context.Response.Headers.TriggerShowEditSuccessMessage("payroll payment", payrollPaymentId);

        return await EditPayrollPayment.HandlePage(runner, dbContext, payrollPaymentId);
    }

    public static async Task<RazorComponentResult> HandlePage(
    [FromServices] SqlKataQueryRunner runner,
    [FromServices] ApplicationDbContext dbContext,
    [FromRoute] Guid payrollPaymentId)
    {
        var result = await new GetPayrollPayment.Runner(runner).Run(new GetPayrollPayment.Query() { PayrollPaymentId = payrollPaymentId });

        var moneyExchanges = await dbContext.Set<MoneyExchange>().AsNoTracking().ToListAsync();

        return new RazorComponentResult<EditPayrollPaymentPage>(new
        {
            Result = result,
            MoneyExchanges = moneyExchanges
        });
    }
}
