using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rebus.Bus;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.ExceptionHandling;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Infrastructure.Ui;

namespace WebAPI.Proformas;

public static class IssueProforma
{
    public class Command
    {
        public DateTime IssuedAt { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
        }
    }

    public class ProformaIssued
    {
        public Guid ProformaId { get; set; }
    }


    public static async Task<Ok> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext dbContext,
    [FromRoute] Guid proformaId,
    [FromServices] IBus bus,
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

            proforma.Issue(command.IssuedAt);
        });

        await bus.Publish(new ProformaIssued() { ProformaId = proformaId });

        return TypedResults.Ok();
    }

    public static Task<RazorComponentResult> HandlePage(
    [FromRoute] Guid proformaId,
    HttpContext context)
    {
        context.Response.Headers.TriggerOpenModal();

        return Task.FromResult<RazorComponentResult>(new RazorComponentResult<IssueProformaPage>(new
        {
            ProformaId = proformaId,
        }));
    }

    public static async Task<RazorComponentResult> HandleAction(
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext dbContext,
    [FromServices] SqlKataQueryRunner runner,
    [FromServices] IBus bus,
    [FromBody] Command command,
    Guid proformaId,
    HttpContext context)
    {
        await Handle(behavior, dbContext, proformaId, bus, command);

        context.Response.Headers.TriggerShowSuccessMessageAndCloseModal("proforma", "issued", proformaId);

        return await GetProforma.HandlePage(runner, dbContext, proformaId);
    }
}
