using FluentValidation;
using Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.ExceptionHandling;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Infrastructure.Ui;

namespace WebAPI.Collections;

public static class CancelCollection
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
    [FromRoute] Guid collectionId,
    [FromServices] IClock clock,
    [FromBody] Command command)
    {
        new Validator().ValidateAndThrow(command);

        await behavior.Handle(async () =>
        {
            var collection = await dbContext.Get<Collection>(collectionId);

            if (collection == null)
            {
                throw new NotFoundException<Collection>();
            }

            collection.Cancel(clock.Now);
        });

        return TypedResults.Ok();
    }

    public static async Task<RazorComponentResult> HandleAction(
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext dbContext,
    [FromServices] SqlKataQueryRunner runner,
    [FromServices] IClock clock,
    HttpContext context,
    Guid collectionId)
    {
        var command = new Command()
        {
        };

        await Handle(behavior, dbContext, collectionId, clock, command);

        context.Response.Headers.TriggerShowSuccessMessage("collection", "canceled", collectionId);

        return await GetCollection.HandlePage(runner, collectionId);
    }
}
