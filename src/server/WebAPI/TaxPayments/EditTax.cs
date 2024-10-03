using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.ExceptionHandling;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Infrastructure.Ui;

namespace WebAPI.TaxPayments;

public static class EditTax
{
    public class Command
    {
        public decimal Amount { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.Amount).GreaterThan(0);
        }
    }

    public static async Task<Ok> Handle(
        [FromServices] TransactionBehavior behavior,
        [FromServices] ApplicationDbContext dbContext,
        [FromRoute] Guid taxPaymentId,
        [FromRoute] int taxPaymentItemId,
        [FromBody] Command command)
    {
        new Validator().ValidateAndThrow(command);

        await behavior.Handle(async () =>
        {
            var taxPayment = await dbContext.Set<TaxPayment>()
                .Include(p => p.Items)
                .FirstAsync(p => p.TaxPaymentId == taxPaymentId);

            if (taxPayment == null)
            {
                throw new NotFoundException<TaxPayment>();
            }

            taxPayment.Edit(taxPaymentItemId, command.Amount);
        });

        return TypedResults.Ok();
    }

    public static async Task<RazorComponentResult> HandlePage(
        [FromRoute] Guid taxPaymentId,
        [FromRoute] int taxPaymentItemId,
        ApplicationDbContext dbContext,
        HttpContext context)
    {
        var taxPayment = await dbContext.Set<TaxPayment>()
            .Include(p => p.Items)
            .FirstAsync(p => p.TaxPaymentId == taxPaymentId);

        context.Response.Headers.TriggerOpenModal();

        return new RazorComponentResult<EditTaxPage>(new { TaxPaymentItem = taxPayment.Get(taxPaymentItemId) });
    }

    public static async Task<RazorComponentResult> HandleAction(
        [FromServices] TransactionBehavior behavior,
        [FromServices] ApplicationDbContext dbContext,
        [FromBody] Command command,
        [FromRoute] Guid taxPaymentId,
        [FromRoute] int taxPaymentItemId,
        HttpContext httpContext)
    {
        await Handle(behavior, dbContext, taxPaymentId, taxPaymentItemId, command);

        httpContext.Response.Headers.TriggerShowSuccessMessageAndCloseModal("tax", "edited", taxPaymentItemId);

        var taxPayment = await dbContext.Set<TaxPayment>()
            .Include(p => p.Items)
            .FirstAsync(p => p.TaxPaymentId == taxPaymentId);

        return new RazorComponentResult<ListTaxesPage>(new { Items = taxPayment.Items, Total = taxPayment.Total, ReadOnly = taxPayment.Status != TaxPaymentStatus.Pending });
    }
}
