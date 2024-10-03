using FluentValidation;
using Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.ExceptionHandling;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Infrastructure.Ui;

namespace WebAPI.PayrollPayments;

public static class CancelPayrollPayment
{
    public class Command
    {
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
    [FromServices] IClock clock,
    [FromBody] Command command)
    {
        new Validator().ValidateAndThrow(command);

        await behavior.Handle(async () =>
        {
            var collaboratorPayment = await dbContext.Get<PayrollPayment>(payrollPaymentId);

            if (collaboratorPayment == null)
            {
                throw new NotFoundException<PayrollPayment>();
            }

            collaboratorPayment.Cancel(clock.Now);
        });

        return TypedResults.Ok();
    }

    public static async Task<RazorComponentResult> HandleAction(
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext dbContext,
    [FromServices] SqlKataQueryRunner runner,
    Guid payrollPaymentId,
    [FromServices] IClock clock,
    HttpContext context)
    {
        var command = new Command()
        {
        };

        await Handle(behavior, dbContext, payrollPaymentId, clock, command);

        context.Response.Headers.TriggerShowSuccessMessage("payroll payment", "canceled", payrollPaymentId);

        return await EditPayrollPayment.HandlePage(runner, dbContext, payrollPaymentId);
    }
}
