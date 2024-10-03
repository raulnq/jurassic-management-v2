using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Collections;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.Ui;
using WebAPI.Infrastructure.SqlKata;


namespace WebAPI.Collections;

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
    [FromServices] CollectionStorage storage,
    [FromRoute] Guid collectionId,
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
                var collection = await dbContext.Get<Collection>(collectionId);

                collection.UploadDocument(command.DocumentUrl!);
            });

            return TypedResults.Ok();
        }
    }

    public static Task<RazorComponentResult> HandlePage(
    [FromRoute] Guid collectionId,
    HttpContext context)
    {
        context.Response.Headers.TriggerOpenModal();

        return Task.FromResult<RazorComponentResult>(new RazorComponentResult<UploadDocumentPage>(new
        {
            CollectionId = collectionId,
        }));
    }

    public static async Task<RazorComponentResult> HandleAction(
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext dbContext,
    [FromServices] CollectionStorage storage,
    [FromServices] SqlKataQueryRunner runner,
    IFormFile file,
    Guid collectionId,
    HttpContext context)
    {
        await Handle(behavior, dbContext, storage, collectionId, file);

        context.Response.Headers.TriggerShowSuccessMessageAndCloseModal("document for the collection", "uploaded", collectionId);

        return await GetCollection.HandlePage(runner, collectionId);
    }
}
