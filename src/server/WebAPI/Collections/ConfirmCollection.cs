using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Infrastructure.Ui;


namespace WebAPI.Collections;

public static class ConfirmCollection
{
    public class Command
    {
        public decimal Total { get; set; }
        public decimal Commission { get; set; }
        public string Number { get; set; } = default!;
        public DateTime ConfirmedAt { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.Number).NotEmpty().MaximumLength(50);
            RuleFor(command => command.Total).GreaterThan(0);
            RuleFor(command => command.Commission).GreaterThanOrEqualTo(0);
        }
    }

    public static async Task<Ok> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext dbContext,
    [FromRoute] Guid collectionId,
    [FromBody] Command command)
    {
        new Validator().ValidateAndThrow(command);

        await behavior.Handle(async () =>
        {
            var collection = await dbContext.Get<Collection>(collectionId);

            collection.Confirm(command.Total, command.Commission, command.Number, command.ConfirmedAt);
        });

        return TypedResults.Ok();
    }

    public static Task<RazorComponentResult> HandlePage(
    [FromRoute] Guid collectionId,
    HttpContext context)
    {
        context.Response.Headers.TriggerOpenModal();

        return Task.FromResult<RazorComponentResult>(new RazorComponentResult<ConfirmCollectionPage>(new
        {
            CollectionId = collectionId,
        }));
    }

    public static async Task<RazorComponentResult> HandleAction(
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext dbContext,
    [FromServices] SqlKataQueryRunner runner,
    [FromBody] Command command,
    Guid collectionId,
    HttpContext context)
    {
        await Handle(behavior, dbContext, collectionId, command);

        context.Response.Headers.TriggerShowSuccessMessageAndCloseModal("collection", "confirmed", collectionId);

        return await GetCollection.HandlePage(runner, collectionId);
    }
}