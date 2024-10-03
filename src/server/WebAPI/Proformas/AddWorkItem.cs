using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using WebAPI.CollaboratorRoles;
using WebAPI.Collaborators;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.ExceptionHandling;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Infrastructure.Ui;

namespace WebAPI.Proformas;

public static class AddWorkItem
{
    public class Command
    {
        [JsonIgnore]
        public Guid ProformaId { get; set; }
        [JsonIgnore]
        public int Week { get; set; }
        public Guid CollaboratorId { get; set; }
        public Guid CollaboratorRoleId { get; set; }
        public decimal Hours { get; set; }
        public decimal FreeHours { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.ProformaId).NotEmpty();
            RuleFor(command => command.CollaboratorId).NotEmpty();
            RuleFor(command => command.CollaboratorRoleId).NotEmpty();
            RuleFor(command => command.Week).GreaterThan(0);
            RuleFor(command => command.Hours).GreaterThan(0);
            RuleFor(command => command.FreeHours).GreaterThanOrEqualTo(0);
        }
    }

    public class Handler
    {
        private readonly ApplicationDbContext _context;

        public Handler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Handle(Command command)
        {
            var proforma = await _context.Set<Proforma>()
                .Include(p => p.Weeks)
                .ThenInclude(w => w.WorkItems)
                .SingleOrDefaultAsync(p => p.ProformaId == command.ProformaId);

            if (proforma == null)
            {
                throw new NotFoundException<Proforma>();
            }

            var collaborator = await _context.Get<Collaborator>(command.CollaboratorId);

            var collaboratorRole = await _context.Get<CollaboratorRole>(command.CollaboratorRoleId);

            proforma.AddWorkItem(command.Week, collaborator, collaboratorRole, command.Hours, command.FreeHours);
        }
    }
    public static async Task<Ok> Handle(
        [FromServices] TransactionBehavior behavior,
        [FromServices] ApplicationDbContext dbContext,
        [FromRoute] Guid proformaId,
        [FromRoute] int week,
        [FromBody] Command command)
    {
        command.ProformaId = proformaId;

        command.Week = week;

        new Validator().ValidateAndThrow(command);

        var handler = new Handler(dbContext);

        await behavior.Handle(() => handler.Handle(command));

        return TypedResults.Ok();
    }

    public static async Task<RazorComponentResult> HandlePage(
        [FromServices] ApplicationDbContext dbContext,
        [FromRoute] Guid proformaId,
        [FromRoute] int week,
        HttpContext context)
    {
        context.Response.Headers.TriggerOpenModal();

        var collaboratorRoles = await dbContext.Set<CollaboratorRole>().AsNoTracking().ToListAsync();

        var collaborators = await dbContext.Set<Collaborator>().AsNoTracking().ToListAsync();

        return new RazorComponentResult<AddWorkItemPage>(new
        {
            ProformaId = proformaId,
            Week = week,
            CollaboratorRoles = collaboratorRoles,
            Collaborators = collaborators
        });
    }

    public static async Task<RazorComponentResult> HandleAction(
        [FromServices] TransactionBehavior behavior,
        [FromServices] SqlKataQueryRunner runner,
        [FromServices] ApplicationDbContext dbContext,
        [FromBody] Command command,
        [FromRoute] Guid proformaId,
        [FromRoute] int week,
        HttpContext context)
    {
        await Handle(behavior, dbContext, proformaId, week, command);

        context.Response.Headers.TriggerShowSuccessMessageAndCloseModal($"collaborator", "added", command.CollaboratorId);

        return await ListProformaWeekWorkItems.HandlePage(new ListProformaWeekWorkItems.Query()
        {
            ProformaId = proformaId,
            Week = week
        }, runner, dbContext, proformaId, week);
    }

}
