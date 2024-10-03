using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.ExceptionHandling;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Infrastructure.Ui;

namespace WebAPI.Proformas;

public static class EditWorkItem
{
    public class Command
    {
        public decimal Hours { get; set; }
        public decimal FreeHours { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.Hours).GreaterThan(0);
            RuleFor(command => command.FreeHours).GreaterThanOrEqualTo(0);
        }
    }

    public static async Task<Ok> Handle(
        [FromServices] TransactionBehavior behavior,
        [FromServices] ApplicationDbContext dbContext,
        [FromRoute] Guid proformaId,
        [FromRoute] int week,
        [FromRoute] Guid collaboratorId,
        [FromBody] Command command)
    {
        new Validator().ValidateAndThrow(command);

        await behavior.Handle(async () =>
        {
            var proforma = await dbContext.Set<Proforma>()
            .Include(p => p.Weeks)
            .ThenInclude(w => w.WorkItems)
            .SingleOrDefaultAsync(p => p.ProformaId == proformaId);

            if (proforma == null)
            {
                throw new NotFoundException<Proforma>();
            }

            proforma.EditWorkItem(week, collaboratorId, command.Hours, command.FreeHours);
        });

        return TypedResults.Ok();
    }

    public static async Task<RazorComponentResult> HandlePage(
        Guid proformaId,
        int week,
        Guid collaboratorId,
        ApplicationDbContext dbContext,
        HttpContext context)
    {
        var proformaWeekWorkItem = await dbContext.Set<ProformaWeekWorkItem>().AsNoTracking().FirstAsync(i => i.ProformaId == proformaId && i.Week == week && i.CollaboratorId == collaboratorId);

        context.Response.Headers.TriggerOpenModal();

        return new RazorComponentResult<EditWorkItemPage>(new { ProformaWeekWorkItem = proformaWeekWorkItem });
    }

    public static async Task<RazorComponentResult> HandleAction(
        [FromServices] TransactionBehavior behavior,
        [FromServices] ApplicationDbContext dbContext,
        [FromServices] SqlKataQueryRunner runner,
        [FromBody] Command command,
        Guid proformaId,
        int week,
        Guid collaboratorId,
        HttpContext httpContext)
    {
        await Handle(behavior, dbContext, proformaId, week, collaboratorId, command);

        httpContext.Response.Headers.TriggerShowSuccessMessageAndCloseModal("collaborator", "edited", collaboratorId);

        return await ListProformaWeekWorkItems.HandlePage(
            new ListProformaWeekWorkItems.Query()
            {
                ProformaId = proformaId,
                Week = week
            }, runner, dbContext, proformaId, week);
    }
}
