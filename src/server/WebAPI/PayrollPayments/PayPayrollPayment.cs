using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Infrastructure.Ui;


namespace WebAPI.PayrollPayments;

public static class PayPayrollPayment
{
    public class Command
    {
        public DateTime PaidAt { get; set; }
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

            payment.Pay(command.PaidAt);
        });

        return TypedResults.Ok();
    }

    public static Task<RazorComponentResult> HandlePage(
    [FromRoute] Guid payrollPaymentId,
    HttpContext context)
    {
        context.Response.Headers.TriggerOpenModal();

        return Task.FromResult<RazorComponentResult>(new RazorComponentResult<PayPayrollPaymentPage>(new
        {
            PayrollPaymentId = payrollPaymentId,
        }));
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

        context.Response.Headers.TriggerShowSuccessMessageAndCloseModal("payroll payment", "paid", payrollPaymentId);

        return await EditPayrollPayment.HandlePage(runner, dbContext, payrollPaymentId);
    }
}
