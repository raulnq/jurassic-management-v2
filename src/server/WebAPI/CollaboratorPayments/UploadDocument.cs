using CollaboratorPayments;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Infrastructure.Ui;


namespace WebAPI.CollaboratorPayments;

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
    [FromServices] CollaboratorPaymentStorage storage,
    [FromRoute] Guid collaboratorPaymentId,
    IFormFile file)
    {
        using (var stream = file.OpenReadStream())
        {
            var ext = Path.GetExtension(file.FileName);

            var url = await storage.Upload($"{Guid.NewGuid()}{ext}".ToString(), stream);

            var command = new Command
            {
                DocumentUrl = url
            };

            new Validator().ValidateAndThrow(command);

            await behavior.Handle(async () =>
            {
                var payment = await dbContext.Get<CollaboratorPayment>(collaboratorPaymentId);

                payment.Upload(command.DocumentUrl!);
            });

            return TypedResults.Ok();
        }
    }

    public static Task<RazorComponentResult> HandlePage(
    [FromRoute] Guid collaboratorPaymentId,
    HttpContext context)
    {
        context.Response.Headers.TriggerOpenModal();

        return Task.FromResult<RazorComponentResult>(new RazorComponentResult<UploadDocumentPage>(new
        {
            CollaboratorPaymentId = collaboratorPaymentId,
        }));
    }

    public static async Task<RazorComponentResult> HandleAction(
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext dbContext,
    [FromServices] SqlKataQueryRunner runner,
    [FromServices] CollaboratorPaymentStorage storage,
    IFormFile file,
    Guid collaboratorPaymentId,
    HttpContext context)
    {
        await Handle(behavior, dbContext, storage, collaboratorPaymentId, file);

        context.Response.Headers.TriggerShowSuccessMessageAndCloseModal("document for the collaborator payment", "uploaded", collaboratorPaymentId);

        return await EditCollaboratorPayment.HandlePage(runner, collaboratorPaymentId);
    }
}
