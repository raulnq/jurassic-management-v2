using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Infrastructure.Ui;


namespace WebAPI.CollaboratorPayments;

public static class PayCollaboratorPayment
{
    public class Command
    {
        public DateTime PaidAt { get; set; }
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
    [FromBody] Command command)
    {
        new Validator().ValidateAndThrow(command);

        await behavior.Handle(async () =>
        {
            var payment = await dbContext.Get<CollaboratorPayment>(collaboratorPaymentId);

            payment.Pay(command.PaidAt);
        });

        return TypedResults.Ok();
    }

    public static Task<RazorComponentResult> HandlePage(
    [FromRoute] Guid collaboratorPaymentId,
    HttpContext context)
    {
        context.Response.Headers.TriggerOpenModal();

        return Task.FromResult<RazorComponentResult>(new RazorComponentResult<PayCollaboratorPaymentPage>(new
        {
            CollaboratorPaymentId = collaboratorPaymentId,
        }));
    }

    public static async Task<RazorComponentResult> HandleAction(
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext dbContext,
    [FromServices] SqlKataQueryRunner runner,
    [FromBody] Command command,
    Guid collaboratorPaymentId,
    HttpContext context)
    {
        await Handle(behavior, dbContext, collaboratorPaymentId, command);

        context.Response.Headers.TriggerShowSuccessMessageAndCloseModal("collaborator payment", "paid", collaboratorPaymentId);

        return await EditCollaboratorPayment.HandlePage(runner, collaboratorPaymentId);
    }
}