using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.Ui;

namespace WebAPI.Collaborators;

public static class EditCollaborator
{
    public class Command
    {
        public string Name { get; set; } = default!;
        public decimal WithholdingPercentage { get; set; }
        public string Email { get; set; } = default!;
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.Name).MaximumLength(100).NotEmpty();
            RuleFor(command => command.Email).MaximumLength(255);
            RuleFor(command => command.WithholdingPercentage).GreaterThanOrEqualTo(0).LessThanOrEqualTo(100);
        }
    }

    public static async Task<Ok> Handle(
        [FromServices] TransactionBehavior behavior,
        [FromServices] ApplicationDbContext dbContext,
        [FromRoute] Guid collaboratorId,
        [FromBody] Command command)
    {
        new Validator().ValidateAndThrow(command);

        await behavior.Handle(async () =>
        {
            var collaborator = await dbContext.Get<Collaborator>(collaboratorId);

            collaborator.Edit(command.Name, command.WithholdingPercentage, command.Email);
        });

        return TypedResults.Ok();
    }

    public static async Task<RazorComponentResult> HandlePage(
        [FromServices] ApplicationDbContext dbContext,
        [FromRoute] Guid collaboratorId)
    {
        var collaborator = await dbContext.Set<Collaborator>().AsNoTracking().FirstAsync(cr => cr.CollaboratorId == collaboratorId);

        return new RazorComponentResult<EditCollaboratorPage>(new { Collaborator = collaborator });
    }

    public static async Task<RazorComponentResult> HandleAction(
        [FromServices] TransactionBehavior behavior,
        [FromServices] ApplicationDbContext dbContext,
        [FromRoute] Guid collaboratorId,
        [FromBody] Command command,
        HttpContext httpContext)
    {
        await Handle(behavior, dbContext, collaboratorId, command);

        httpContext.Response.Headers.TriggerShowEditSuccessMessage("collaborator", collaboratorId);

        return await HandlePage(dbContext, collaboratorId);
    }
}