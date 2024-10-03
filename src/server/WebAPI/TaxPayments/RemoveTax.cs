using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.EntityFrameworkCore;
using WebAPI.CollaboratorPayments;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Invoices;
using WebAPI.PayrollPayments;

namespace WebAPI.TaxPayments;

public static class RemoveTax
{
    public static async Task<Ok> Handle(
        [FromServices] TransactionBehavior behavior,
        [FromServices] ApplicationDbContext dbContext,
        [FromRoute] Guid taxPaymentId,
        [FromRoute] int taxPaymentItemId)
    {
        await behavior.Handle(async () =>
        {
            var taxPayment = await dbContext.Set<TaxPayment>()
                .Include(p => p.Items)
                .FirstAsync(p => p.TaxPaymentId == taxPaymentId);

            var collaboratorPayments = await dbContext.Set<CollaboratorPayment>()
               .Where(cp => cp.TaxPaymentId == taxPayment.TaxPaymentId)
            .ToListAsync();

            var invoices = await dbContext.Set<Invoice>()
                .Where(i => i.TaxPaymentId == taxPayment.TaxPaymentId)
                .ToListAsync();

            var payrollPayments = await dbContext.Set<PayrollPayment>()
                .Where(pp => pp.TaxPaymentId == taxPayment.TaxPaymentId)
                .ToListAsync();

            taxPayment.Remove(taxPaymentItemId);

            foreach (var item in collaboratorPayments)
            {
                item.RemoveFromTaxPayment();
            }

            foreach (var item in invoices)
            {
                item.RemoveFromTaxPayment();
            }

            foreach (var item in payrollPayments)
            {
                item.RemoveFromTaxPayment();
            }

        });

        return TypedResults.Ok();
    }

    public static async Task<RazorComponentResult> HandleAction(
        [FromServices] TransactionBehavior behavior,
        [FromServices] ApplicationDbContext dbContext,
        [FromRoute] Guid taxPaymentId,
        [FromRoute] int taxPaymentItemId)
    {
        await Handle(behavior, dbContext, taxPaymentId, taxPaymentItemId);

        var taxPayment = await dbContext.Set<TaxPayment>()
            .Include(p => p.Items)
            .FirstAsync(p => p.TaxPaymentId == taxPaymentId);

        return new RazorComponentResult<ListTaxesPage>(new { Items = taxPayment.Items, Total = taxPayment.Total, ReadOnly = taxPayment.Status != TaxPaymentStatus.Pending });

    }
}
