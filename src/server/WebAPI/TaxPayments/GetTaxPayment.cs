using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Infrastructure.EntityFramework;

namespace WebAPI.TaxPayments;

public static class GetTaxPayment
{
    public static async Task<RazorComponentResult> HandlePage(
        [FromServices] ApplicationDbContext dbContext,
        [FromRoute] Guid taxPaymentId)
    {
        var taxPayment = await dbContext.Set<TaxPayment>().AsNoTracking()
            .Include(p => p.Items)
            .FirstAsync(p => p.TaxPaymentId == taxPaymentId);

        return new RazorComponentResult<GetTaxPaymentPage>(new
        {
            TaxPayment = taxPayment,
        });
    }
}