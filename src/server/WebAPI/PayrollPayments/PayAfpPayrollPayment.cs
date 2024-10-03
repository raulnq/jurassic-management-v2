using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Infrastructure.Ui;


namespace WebAPI.PayrollPayments;

public static class PayAfpPayrollPayment
{
    public class Command
    {
        public DateTime PaidAt { get; set; }
        public Decimal Afp { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
        }
    }

    public static async Task<Ok> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext dbContext,
    [FromRoute] Guid payrollPaymentId,
    [FromBody] Command command)
    {
        new Validator().ValidateAndThrow(command);

        await behavior.Handle(async () =>
        {
            var payment = await dbContext.Get<PayrollPayment>(payrollPaymentId);

            payment.PayAfp(command.PaidAt, command.Afp);
        });

        return TypedResults.Ok();
    }

    public static async Task<RazorComponentResult> HandlePage(
    [FromRoute] Guid payrollPaymentId,
    [FromServices] ApplicationDbContext dbContext,
    HttpContext context)
    {
        context.Response.Headers.TriggerOpenModal();

        var payment = await dbContext.Get<PayrollPayment>(payrollPaymentId);

        return new RazorComponentResult<PayAfpPayrollPaymentPage>(new
        {
            PayrollPaymentId = payrollPaymentId,
            Afp = payment.Afp
        });
    }

    public static async Task<RazorComponentResult> HandleAction(
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext dbContext,
    [FromServices] SqlKataQueryRunner runner,
    [FromBody] Command command,
    Guid payrollPaymentId,
    HttpContext context)
    {
        await Handle(behavior, dbContext, payrollPaymentId, command);

        context.Response.Headers.TriggerShowSuccessMessageAndCloseModal("payroll payment", "afp paid", payrollPaymentId);

        return await EditPayrollPayment.HandlePage(runner, dbContext, payrollPaymentId);
    }
}