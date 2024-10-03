using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Collaborators;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Infrastructure.Ui;
using WebAPI.Proformas;
using WebAPI.ProformaToCollaboratorPaymentProcesses;

namespace WebAPI.CollaboratorPayments;

public static class EditCollaboratorPayment
{
    public class Command
    {
        public decimal GrossSalary { get; set; }
        public Currency Currency { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.GrossSalary).GreaterThan(0);
        }
    }

    public static async Task<Ok> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromRoute] Guid collaboratorPaymentId,
    [FromServices] ApplicationDbContext dbContext,
    [FromBody] Command command)
    {
        new Validator().ValidateAndThrow(command);

        await behavior.Handle(async () =>
        {
            var payment = await dbContext.Get<CollaboratorPayment>(collaboratorPaymentId);

            var collaborator = await dbContext.Set<Collaborator>().AsNoTracking().FirstAsync(cr => cr.CollaboratorId == payment.CollaboratorId);

            payment.Edit(command.GrossSalary, command.Currency, collaborator.WithholdingPercentage);
        });

        return TypedResults.Ok();
    }

    public static async Task<RazorComponentResult> HandleAction(
    [FromServices] TransactionBehavior behavior,
    [FromServices] SqlKataQueryRunner runner,
    [FromBody] Command command,
    [FromServices] ApplicationDbContext dbContext,
    Guid collaboratorPaymentId,
    HttpContext context)
    {
        await Handle(behavior, collaboratorPaymentId, dbContext, command);

        context.Response.Headers.TriggerShowEditSuccessMessage("collaborator payment", collaboratorPaymentId);

        return await EditCollaboratorPayment.HandlePage(runner, collaboratorPaymentId);
    }

    public static async Task<RazorComponentResult> HandlePage(
    [FromServices] SqlKataQueryRunner runner,
    [FromRoute] Guid collaboratorPaymentId)
    {
        var result = await new GetCollaboratorPayment.Runner(runner).Run(new GetCollaboratorPayment.Query() { CollaboratorPaymentId = collaboratorPaymentId });

        var listProformaToCollaboratorPaymentProcessItemsQuery = new ListProformaToCollaboratorPaymentProcessItems.Query() { CollaboratorPaymentId = collaboratorPaymentId, PageSize = 5 };

        var listProformaToCollaboratorPaymentProcessItemsResult = await new ListProformaToCollaboratorPaymentProcessItems.Runner(runner).Run(listProformaToCollaboratorPaymentProcessItemsQuery);

        return new RazorComponentResult<EditCollaboratorPaymentPage>(new
        {
            Result = result,
            ListProformaToCollaboratorPaymentProcessItemsResult = listProformaToCollaboratorPaymentProcessItemsResult,
            ListProformaToCollaboratorPaymentProcessItemsQuery = listProformaToCollaboratorPaymentProcessItemsQuery,
        });
    }
}
