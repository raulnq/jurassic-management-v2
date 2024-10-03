using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.Ui;

namespace WebAPI.CollaboratorRoles;

public static class EditCollaboratorRole
{
    public class Command
    {
        public string Name { get; set; } = default!;
        public decimal FeeAmount { get; set; }
        public decimal ProfitPercentage { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.Name).MaximumLength(100).NotEmpty();
            RuleFor(command => command.FeeAmount).GreaterThanOrEqualTo(0);
            RuleFor(command => command.ProfitPercentage).GreaterThanOrEqualTo(0).LessThanOrEqualTo(100);
        }
    }

    public static async Task<Ok> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext dbContext,
    [FromRoute] Guid collaboratorRoleId,
    [FromBody] Command command)
    {
        new Validator().ValidateAndThrow(command);

        await behavior.Handle(async () =>
        {
            var collaboratorRole = await dbContext.Get<CollaboratorRole>(collaboratorRoleId);

            collaboratorRole.Edit(command.Name!, command.FeeAmount, command.ProfitPercentage);
        });

        return TypedResults.Ok();
    }

    public static async Task<RazorComponentResult> HandlePage(
    [FromServices] ApplicationDbContext dbContext,
    [FromRoute] Guid collaboratorRoleId)
    {
        var collaboratorRole = await dbContext.Set<CollaboratorRole>().AsNoTracking().FirstAsync(cr => cr.CollaboratorRoleId == collaboratorRoleId);

        return new RazorComponentResult<EditCollaboratorRolePage>(new { CollaboratorRole = collaboratorRole });
    }

    public static async Task<RazorComponentResult> HandleAction(
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext dbContext,
    [FromRoute] Guid collaboratorRoleId,
    [FromBody] Command command,
    HttpContext httpContext)
    {
        await Handle(behavior, dbContext, collaboratorRoleId, command);

        httpContext.Response.Headers.TriggerShowEditSuccessMessage("collaborator role", collaboratorRoleId);

        return await HandlePage(dbContext, collaboratorRoleId);
    }
}