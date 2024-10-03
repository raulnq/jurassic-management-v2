using FluentValidation;
using Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.ExceptionHandling;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Infrastructure.Ui;

namespace WebAPI.CollaboratorPayments;

public static class CancelCollaboratorPayment
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
    [FromRoute] Guid collaboratorPaymentId,
    [FromServices] IClock clock,
    [FromBody] Command command)
    {
        new Validator().ValidateAndThrow(command);

        await behavior.Handle(async () =>
        {
            var collaboratorPayment = await dbContext.Get<CollaboratorPayment>(collaboratorPaymentId);

            if (collaboratorPayment == null)
            {
                throw new NotFoundException<CollaboratorPayment>();
            }

            collaboratorPayment.Cancel(clock.Now);
        });

        return TypedResults.Ok();
    }

    public static async Task<RazorComponentResult> HandleAction(
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext dbContext,
    [FromServices] SqlKataQueryRunner runner,
    Guid collaboratorPaymentId,
    [FromServices] IClock clock,
    HttpContext context)
    {
        var command = new Command()
        {
        };

        await Handle(behavior, dbContext, collaboratorPaymentId, clock, command);

        context.Response.Headers.TriggerShowSuccessMessage("collaborator payment", "canceled", collaboratorPaymentId);

        return await EditCollaboratorPayment.HandlePage(runner, collaboratorPaymentId);
    }
}
