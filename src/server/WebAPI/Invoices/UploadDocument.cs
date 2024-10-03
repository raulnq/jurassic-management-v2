using FluentValidation;
using Invoices;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Infrastructure.Ui;


namespace WebAPI.Invoices;

public static class UploadDocument
{
    public class Command
    {
        public Guid InvoiceId { get; set; }
        public string? DocumentUrl { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.DocumentUrl).NotEmpty().MaximumLength(500);
            RuleFor(command => command.InvoiceId).NotEmpty();
        }
    }

    public static async Task<Ok> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext dbContext,
    [FromServices] InvoiceStorage storage,
    [FromRoute] Guid invoiceId,
    IFormFile file)
    {
        using (var stream = file.OpenReadStream())
        {
            var ext = Path.GetExtension(file.FileName);

            var url = await storage.Upload($"{Guid.NewGuid()}{ext}".ToString(), stream);

            var command = new Command
            {
                InvoiceId = invoiceId,
                DocumentUrl = url
            };

            new Validator().ValidateAndThrow(command);

            await behavior.Handle(async () =>
            {
                var invoice = await dbContext.Get<Invoice>(command.InvoiceId);

                invoice.UploadDocument(command.DocumentUrl!);
            });

            return TypedResults.Ok();
        }
    }

    public static Task<RazorComponentResult> HandlePage(
    [FromRoute] Guid invoiceId,
    HttpContext context)
    {
        context.Response.Headers.TriggerOpenModal();

        return Task.FromResult<RazorComponentResult>(new RazorComponentResult<UploadDocumentPage>(new
        {
            InvoiceId = invoiceId,
        }));
    }

    public static async Task<RazorComponentResult> HandleAction(
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext dbContext,
    [FromServices] SqlKataQueryRunner runner,
    [FromServices] InvoiceStorage storage,
    IFormFile file,
    Guid invoiceId,
    HttpContext context)
    {
        await Handle(behavior, dbContext, storage, invoiceId, file);

        context.Response.Headers.TriggerShowSuccessMessageAndCloseModal("document for the invoice", "uploaded", invoiceId);

        return await GetInvoice.HandlePage(runner, invoiceId);
    }
}
