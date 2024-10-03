using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.ExceptionHandling;
using WebAPI.Infrastructure.SqlKata;

namespace WebAPI.Proformas;

public static class RemoveWorkItem
{
    public class Command
    {
        [JsonIgnore]
        public Guid ProformaId { get; set; }
        [JsonIgnore]
        public int Week { get; set; }
        [JsonIgnore]
        public Guid CollaboratorId { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.ProformaId).NotEmpty();
            RuleFor(command => command.CollaboratorId).NotEmpty();
            RuleFor(command => command.Week).GreaterThan(0);
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

            proforma.RemoveWorkItem(command.Week, command.CollaboratorId);
        }
    }


    public static async Task<Ok> Handle(
        [FromServices] TransactionBehavior behavior,
        [FromServices] ApplicationDbContext dbContext,
        [FromRoute] Guid proformaId,
        [FromRoute] int week,
        [FromRoute] Guid collaboratorId)
    {
        var command = new Command()
        {
            ProformaId = proformaId,
            Week = week,
            CollaboratorId = collaboratorId
        };

        new Validator().ValidateAndThrow(command);

        var handler = new Handler(dbContext);

        await behavior.Handle(() => handler.Handle(command));

        return TypedResults.Ok();
    }

    public static async Task<RazorComponentResult> HandleAction(
        [FromServices] TransactionBehavior behavior,
        [FromServices] SqlKataQueryRunner runner,
        [FromServices] ApplicationDbContext dbContext,
        Guid proformaId,
        int week,
        Guid collaboratorId)
    {
        await Handle(behavior, dbContext, proformaId, week, collaboratorId);

        return await ListProformaWeekWorkItems.HandlePage(
            new ListProformaWeekWorkItems.Query()
            {
                ProformaId = proformaId,
                Week = week
            },
            runner,
            dbContext,
            proformaId,
            week);

    }
}
