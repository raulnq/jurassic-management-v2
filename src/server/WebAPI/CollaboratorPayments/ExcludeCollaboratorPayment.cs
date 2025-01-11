using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.ExceptionHandling;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Infrastructure.Ui;

namespace WebAPI.CollaboratorPayments;

public static class ExcludeCollaboratorPayment
{
    public static async Task<Ok> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext dbContext,
    [FromRoute] Guid collaboratorPaymentId)
    {
        await behavior.Handle(async () =>
        {
            var collaboratorPayment = await dbContext.Get<CollaboratorPayment>(collaboratorPaymentId);

            if (collaboratorPayment == null)
            {
                throw new NotFoundException<CollaboratorPayment>();
            }

            collaboratorPayment.Exclude();
        });

        return TypedResults.Ok();
    }

    public static async Task<RazorComponentResult> HandleAction(
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext dbContext,
    [FromServices] SqlKataQueryRunner runner,
    Guid collaboratorPaymentId,
    HttpContext context)
    {
        await Handle(behavior, dbContext, collaboratorPaymentId);

        context.Response.Headers.TriggerShowSuccessMessage("collaborator payment", "excluded", collaboratorPaymentId);

        return await EditCollaboratorPayment.HandlePage(runner, collaboratorPaymentId);
    }
}
