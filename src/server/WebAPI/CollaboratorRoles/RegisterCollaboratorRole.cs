using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Infrastructure.Ui;

namespace WebAPI.CollaboratorRoles;

public static class RegisterCollaboratorRole
{
    public class Command
    {
        public string? Name { get; set; }
        public decimal FeeAmount { get; set; }
        public decimal ProfitPercentage { get; set; }
    }

    public class Result
    {
        public Guid CollaboratorRoleId { get; set; }
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

    public static async Task<Ok<Result>> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext context,
    [FromBody] Command command)
    {
        new Validator().ValidateAndThrow(command);

        var result = await behavior.Handle(() =>
        {
            var collaboratorRole = new CollaboratorRole(NewId.Next().ToSequentialGuid(), command.Name!, command.FeeAmount, command.ProfitPercentage);

            context.Set<CollaboratorRole>().Add(collaboratorRole);

            return Task.FromResult(new Result()
            {
                CollaboratorRoleId = collaboratorRole.CollaboratorRoleId
            });
        });

        return TypedResults.Ok(result);
    }

    public static Task<RazorComponentResult> HandlePage()
    {
        return Task.FromResult<RazorComponentResult>(new RazorComponentResult<RegisterCollaboratorRolePage>(new { }));
    }

    public static async Task<RazorComponentResult> HandleAction(
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext dbContext,
    [FromServices] SqlKataQueryRunner runner,
    [FromBody] Command command,
    HttpContext httpContext)
    {
        var result = await Handle(behavior, dbContext, command);

        httpContext.Response.Headers.TriggerShowRegisterSuccessMessage("collaborator role", result.Value!.CollaboratorRoleId);

        return await ListCollaboratorRoles.HandlePage(new ListCollaboratorRoles.Query() { }, runner);
    }
}
