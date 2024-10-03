using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.ExceptionHandling;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Infrastructure.Ui;

namespace WebAPI.PayrollPayments;

public static class ExcludePayrollPayment
{
    public static async Task<Ok> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext dbContext,
    [FromRoute] Guid payrollPaymentId)
    {
        await behavior.Handle(async () =>
        {
            var collaboratorPayment = await dbContext.Get<PayrollPayment>(payrollPaymentId);

            if (collaboratorPayment == null)
            {
                throw new NotFoundException<PayrollPayment>();
            }

            collaboratorPayment.Exclude();
        });

        return TypedResults.Ok();
    }

    public static async Task<RazorComponentResult> HandleAction(
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext dbContext,
    [FromServices] SqlKataQueryRunner runner,
    Guid payrollPaymentId,
    HttpContext context)
    {
        await Handle(behavior, dbContext, payrollPaymentId);

        context.Response.Headers.TriggerShowSuccessMessage("payroll payment", "excluded", payrollPaymentId);

        return await EditPayrollPayment.HandlePage(runner, dbContext, payrollPaymentId);
    }
}
