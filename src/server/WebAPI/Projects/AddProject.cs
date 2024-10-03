using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Infrastructure.Ui;

namespace WebAPI.Projects;

public static class AddProject
{
    public class Command
    {
        public string Name { get; set; } = default!;
    }

    public class Result
    {
        public Guid ProjectId { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.Name).MaximumLength(100).NotEmpty();
        }
    }

    public static async Task<Ok<Result>> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext dbContext,
    [FromRoute] Guid clientId,
    [FromBody] Command command)
    {
        new Validator().ValidateAndThrow(command);

        var result = await behavior.Handle(() =>
        {
            var project = new Project(NewId.Next().ToSequentialGuid(), clientId, command.Name!);

            dbContext.Set<Project>().Add(project);

            return Task.FromResult(new Result()
            {
                ProjectId = project.ProjectId
            });
        });

        return TypedResults.Ok(result);
    }

    public static Task<RazorComponentResult> HandlePage(Guid clientId, HttpContext context)
    {
        context.Response.Headers.TriggerOpenModal();

        return Task.FromResult<RazorComponentResult>(new RazorComponentResult<AddProjectPage>(new { ClientId = clientId }));
    }

    public static async Task<RazorComponentResult> HandleAction(
        [FromServices] TransactionBehavior behavior,
        [FromServices] ApplicationDbContext dbContext,
        [FromServices] SqlKataQueryRunner runner,
        [FromBody] Command command,
        Guid clientId,
        HttpContext context)
    {
        var result = await Handle(behavior, dbContext, clientId, command);

        context.Response.Headers.TriggerShowRegisterSuccessMessageAndCloseModal("project", result.Value!.ProjectId);

        return await ListProjects.HandlePage(new ListProjects.Query() { ClientId = clientId }, clientId, runner);
    }

}
