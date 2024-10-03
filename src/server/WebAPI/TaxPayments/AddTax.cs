using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.Ui;

namespace WebAPI.TaxPayments;

public static class AddTax
{
    public class Command
    {
        public TaxType Type { get; set; }
        public decimal Amount { get; set; }
    }

    public class Result
    {
        public int TaxPaymentItemId { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.Amount).GreaterThan(0);
        }
    }

    public static async Task<Ok<Result>> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext dbContext,
    [FromRoute] Guid taxPaymentId,
    [FromBody] Command command)
    {
        new Validator().ValidateAndThrow(command);

        var result = await behavior.Handle(async () =>
        {
            var taxPayment = await dbContext.Set<TaxPayment>()
                .Include(p => p.Items)
                .FirstAsync(p => p.TaxPaymentId == taxPaymentId);

            var taxPaymentItemId = taxPayment.Add(command.Amount, command.Type);

            return new Result()
            {
                TaxPaymentItemId = taxPaymentItemId
            };
        });

        return TypedResults.Ok(result);
    }

    public static Task<RazorComponentResult> HandlePage(
        [FromRoute] Guid taxPaymentId,
        HttpContext context)
    {
        context.Response.Headers.TriggerOpenModal();

        return Task.FromResult<RazorComponentResult>(new RazorComponentResult<AddTaxPage>(new { TaxPaymentId = taxPaymentId }));
    }

    public static async Task<RazorComponentResult> HandleAction(
        [FromServices] TransactionBehavior behavior,
        [FromServices] ApplicationDbContext dbContext,
        [FromBody] Command command,
        Guid taxPaymentId,
        HttpContext context)
    {
        var result = await Handle(behavior, dbContext, taxPaymentId, command);

        context.Response.Headers.TriggerShowRegisterSuccessMessageAndCloseModal("tax", result.Value!.TaxPaymentItemId);

        var taxPayment = await dbContext.Set<TaxPayment>()
            .Include(p => p.Items)
            .FirstAsync(p => p.TaxPaymentId == taxPaymentId);

        return new RazorComponentResult<ListTaxesPage>(new { Items = taxPayment.Items, Total = taxPayment.Total, ReadOnly = taxPayment.Status != TaxPaymentStatus.Pending });
    }

}
