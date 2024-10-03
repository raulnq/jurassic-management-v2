using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MoneyExchanges;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.Ui;


namespace WebAPI.MoneyExchanges;

public static class UploadDocument
{
    public class Command
    {
        public string? DocumentUrl { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.DocumentUrl).NotEmpty().MaximumLength(500);
        }
    }

    public static async Task<Ok> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext dbContext,
    [FromServices] MoneyExchangeStorage storage,
    [FromRoute] Guid moneyExchangeId,
    IFormFile file)
    {
        using (var stream = file.OpenReadStream())
        {
            var ext = Path.GetExtension(file.FileName);

            var url = await storage.Upload($"{Guid.NewGuid()}{ext}".ToString(), stream, file.ContentType);

            var command = new Command
            {
                DocumentUrl = url
            };

            new Validator().ValidateAndThrow(command);

            await behavior.Handle(async () =>
            {
                var moneyExchanges = await dbContext.Get<MoneyExchange>(moneyExchangeId);

                moneyExchanges.UploadDocument(command.DocumentUrl!);
            });

            return TypedResults.Ok();
        }
    }

    public static Task<RazorComponentResult> HandlePage(
    [FromRoute] Guid moneyExchangeId,
    HttpContext context)
    {
        context.Response.Headers.TriggerOpenModal();

        return Task.FromResult<RazorComponentResult>(new RazorComponentResult<UploadDocumentPage>(new
        {
            MoneyExchangeId = moneyExchangeId,
        }));
    }

    public static async Task<RazorComponentResult> HandleAction(
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext dbContext,
    [FromServices] MoneyExchangeStorage storage,
    IFormFile file,
    Guid moneyExchangeId,
    HttpContext context)
    {
        await Handle(behavior, dbContext, storage, moneyExchangeId, file);

        context.Response.Headers.TriggerShowSuccessMessageAndCloseModal("document for the money exchange", "uploaded", moneyExchangeId);

        return await EditMoneyExchange.HandlePage(dbContext, moneyExchangeId);
    }
}
