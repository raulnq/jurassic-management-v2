using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.CollaboratorPayments;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.ExceptionHandling;
using WebAPI.Invoices;
using WebAPI.PayrollPayments;

namespace WebAPI.TaxPayments;

public static class LoadTaxes
{
    public static async Task<Ok> Handle(
    [FromRoute] Guid taxPaymentId,
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext dbContext)
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

            var start = new DateTime(taxPayment.Year, int.Parse(taxPayment.Month!), 1);
            var end = start.AddMonths(1).AddDays(-1);

            var collaboratorPayments = await dbContext.Set<CollaboratorPayment>()
                .Where(cp => cp.PaidAt >= start && cp.PaidAt <= end)
                .ToListAsync();

            var invoices = await dbContext.Set<Invoice>()
                .Where(i => i.IssuedAt >= start && i.IssuedAt <= end)
                .ToListAsync();

            var payrollPayments = await dbContext.Set<PayrollPayment>()
                .Where(pp => pp.PaidAt >= start && pp.PaidAt <= end)
                .ToListAsync();

            var total = 0m;

            foreach (var item in collaboratorPayments.Where(i => i.TaxPaymentId == null))
            {
                total = total + item.GeTaxes();

                item.AddInTaxPayment(taxPayment.TaxPaymentId);
            }

            if (total > 0)
            {
                taxPayment.Add(total, TaxType.CUARTACATEGORIA);
            }

            total = 0;

            foreach (var item in invoices.Where(i => i.TaxPaymentId == null))
            {
                total = total + item.GetTaxes();

                item.AddInTaxPayment(taxPayment.TaxPaymentId);
            }

            if (total > 0)
            {
                taxPayment.Add(total, TaxType.REGIMENMYPE);
            }

            total = 0;

            foreach (var item in payrollPayments.Where(i => i.TaxPaymentId == null))
            {
                total = total + item.GetTaxes();

                item.AddInTaxPayment(taxPayment.TaxPaymentId);
            }

            if (total > 0)
            {
                taxPayment.Add(total, TaxType.ESSALUD);
            }

        });

        return TypedResults.Ok();
    }

    public static async Task<RazorComponentResult> HandleAction(
    [FromServices] ApplicationDbContext dbContext,
    [FromServices] TransactionBehavior behavior,
    Guid taxPaymentId)
    {
        await Handle(taxPaymentId, behavior, dbContext);

        var taxPayment = await dbContext.Set<TaxPayment>()
            .Include(p => p.Items)
            .FirstAsync(p => p.TaxPaymentId == taxPaymentId);

        return new RazorComponentResult<ListTaxesPage>(new { Items = taxPayment.Items, Total = taxPayment.Total, ReadOnly = taxPayment.Status != TaxPaymentStatus.Pending });

    }
}
