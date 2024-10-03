using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Infrastructure.Ui;

namespace WebAPI.Collaborators;

public static class RegisterCollaborator
{
    public class Command
    {
        public string Name { get; set; } = default!;
        public decimal WithholdingPercentage { get; set; }
        public string Email { get; set; } = default!;
    }

    public class Result
    {
        public Guid CollaboratorId { get; set; }
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

    public static async Task<Ok<Result>> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext dbContext,
    [FromBody] Command command)
    {
        new Validator().ValidateAndThrow(command);

        var result = await behavior.Handle(() =>
        {
            var collaborator = new Collaborator(NewId.Next().ToSequentialGuid(), command.Name, command.WithholdingPercentage, command.Email);

            dbContext.Set<Collaborator>().Add(collaborator);

            return Task.FromResult(new Result()
            {
                CollaboratorId = collaborator.CollaboratorId
            });
        });

        return TypedResults.Ok(result);
    }

    public static Task<RazorComponentResult> HandlePage()
    {
        return Task.FromResult<RazorComponentResult>(new RazorComponentResult<RegisterCollaboratorPage>(new { }));
    }

    public static async Task<RazorComponentResult> HandleAction(
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext dbContext,
    [FromServices] SqlKataQueryRunner runner,
    [FromBody] Command command,
    HttpContext httpContext)
    {
        var result = await Handle(behavior, dbContext, command);

        httpContext.Response.Headers.TriggerShowRegisterSuccessMessage("collaborator", result.Value!.CollaboratorId);

        return await ListCollaborators.HandlePage(new ListCollaborators.Query() { }, runner);
    }
}
