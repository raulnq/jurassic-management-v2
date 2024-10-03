using FluentValidation;
using Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.ExceptionHandling;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Infrastructure.Ui;

namespace WebAPI.Invoices;

public static class CancelInvoice
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
    [FromRoute] Guid invoiceId,
    [FromServices] IClock clock,
    [FromBody] Command command)
    {
        new Validator().ValidateAndThrow(command);

        await behavior.Handle(async () =>
        {
            var invoice = await dbContext.Get<Invoice>(invoiceId);

            if (invoice == null)
            {
                throw new NotFoundException<Invoice>();
            }

            invoice.Cancel(clock.Now);
        });

        return TypedResults.Ok();
    }

    public static async Task<RazorComponentResult> HandleAction(
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext dbContext,
    [FromServices] SqlKataQueryRunner runner,
    [FromServices] IClock clock,
    HttpContext context,
    Guid invoiceId)
    {
        var command = new Command()
        {
        };

        await Handle(behavior, dbContext, invoiceId, clock, command);

        context.Response.Headers.TriggerShowSuccessMessage("invoice", "canceled", invoiceId);

        return await GetInvoice.HandlePage(runner, invoiceId);
    }
}
