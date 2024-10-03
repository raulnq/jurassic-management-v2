using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.ExceptionHandling;
using WebAPI.Infrastructure.Ui;

namespace WebAPI.TaxPayments;

public static class PayTaxPayment
{
    public class Command
    {
        public DateTime PaidAt { get; set; }
    }

    public static async Task<Ok> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext dbContext,
    [FromRoute] Guid taxPaymentId,
    [FromBody] Command command)
    {
        await behavior.Handle(async () =>
        {
            var taxPayment = await dbContext.Set<TaxPayment>()
                .Include(p => p.Items)
                .FirstAsync(p => p.TaxPaymentId == taxPaymentId);

            if (taxPayment == null)
            {
                throw new NotFoundException<TaxPayment>();
            }

            taxPayment.Pay(command.PaidAt);
        });

        return TypedResults.Ok();
    }

    public static Task<RazorComponentResult> HandlePage(
    [FromRoute] Guid taxPaymentId,
    HttpContext context)
    {
        context.Response.Headers.TriggerOpenModal();

        return Task.FromResult<RazorComponentResult>(new RazorComponentResult<PayTaxPaymentPage>(new
        {
            TaxPaymentId = taxPaymentId,
        }));
    }

    public static async Task<RazorComponentResult> HandleAction(
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext dbContext,
    [FromBody] Command command,
    Guid taxPaymentId,
    HttpContext context)
    {
        await Handle(behavior, dbContext, taxPaymentId, command);

        context.Response.Headers.TriggerShowSuccessMessageAndCloseModal("tax payment", "paid", taxPaymentId);

        return await GetTaxPayment.HandlePage(dbContext, taxPaymentId);
    }
}
