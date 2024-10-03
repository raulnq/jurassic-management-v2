using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Infrastructure.Ui;

namespace WebAPI.Projects;

public static class EditProject
{
    public class Command
    {
        public string Name { get; set; } = default!;
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.Name).MaximumLength(100).NotEmpty();
        }
    }

    public static async Task<Ok> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext dbContext,
    [FromRoute] Guid projectId,
    [FromRoute] Guid clientId,
    [FromBody] Command command)
    {
        new Validator().ValidateAndThrow(command);

        await behavior.Handle(async () =>
        {
            var project = await dbContext.Get<Project>(projectId);

            project.Edit(command.Name!);
        });

        return TypedResults.Ok();
    }

    public static async Task<RazorComponentResult> HandlePage(
        Guid clientId,
        Guid projectId,
        ApplicationDbContext dbContext,
        HttpContext context)
    {
        var project = await dbContext.Set<Project>().AsNoTracking().FirstAsync(c => c.ProjectId == projectId);

        context.Response.Headers.TriggerOpenModal();

        return new RazorComponentResult<EditProjectPage>(new { Project = project });
    }

    public static async Task<RazorComponentResult> HandleAction(
        [FromServices] TransactionBehavior behavior,
        [FromServices] ApplicationDbContext dbContext,
        [FromServices] SqlKataQueryRunner runner,
        [FromBody] Command command,
        Guid clientId,
        Guid projectId,
        HttpContext context)
    {
        await Handle(behavior, dbContext, projectId, clientId, command);

        context.Response.Headers.TriggerShowEditSuccessMessageAndCloseModal("project", projectId);

        return await ListProjects.HandlePage(new ListProjects.Query() { ClientId = clientId }, clientId, runner);
    }
}