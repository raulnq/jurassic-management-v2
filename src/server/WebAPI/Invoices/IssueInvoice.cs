using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Infrastructure.Ui;


namespace WebAPI.Invoices;

public static class IssueInvoice
{
    public class Command
    {
        public DateTime IssuedAt { get; set; }
        public string Number { get; set; } = default!;
        public decimal ExchangeRate { get; set; }
        public decimal SubTotal { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.Number).NotEmpty().MaximumLength(50);
            RuleFor(command => command.SubTotal).NotEmpty();
        }
    }

    public static async Task<Ok> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext dbContext,
    [FromRoute] Guid invoiceId,
    [FromBody] Command command)
    {
        new Validator().ValidateAndThrow(command);

        await behavior.Handle(async () =>
        {
            var invoice = await dbContext.Get<Invoice>(invoiceId);

            invoice.Issue(command.IssuedAt, command.Number, command.SubTotal, command.ExchangeRate);
        });

        return TypedResults.Ok();
    }

    public static Task<RazorComponentResult> HandlePage(
    [FromRoute] Guid invoiceId,
    HttpContext context)
    {
        context.Response.Headers.TriggerOpenModal();

        return Task.FromResult<RazorComponentResult>(new RazorComponentResult<IssueInvoicePage>(new
        {
            InvoiceId = invoiceId,
        }));
    }

    public static async Task<RazorComponentResult> HandleAction(
        [FromServices] TransactionBehavior behavior,
        [FromServices] ApplicationDbContext bdContext,
        [FromServices] SqlKataQueryRunner runner,
        [FromBody] Command command,
        Guid invoiceId,
        HttpContext context)
    {
        await Handle(behavior, bdContext, invoiceId, command);

        context.Response.Headers.TriggerShowSuccessMessageAndCloseModal("invoice", "issued", invoiceId);

        return await GetInvoice.HandlePage(runner, invoiceId);
    }
}