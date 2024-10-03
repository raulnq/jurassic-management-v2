using FluentValidation;
using Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.ExceptionHandling;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Infrastructure.Ui;

namespace WebAPI.Proformas;

public static class OpenProforma
{
    public class Command
    {
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
        }
    }

    public static async Task<Ok> Handle(
        [FromServices] TransactionBehavior behavior,
        [FromServices] ApplicationDbContext dbContext,
        [FromRoute] Guid proformaId,
        [FromServices] IClock clock,
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

            proforma.Open();
        });

        return TypedResults.Ok();
    }

    public static async Task<RazorComponentResult> HandleAction(
        [FromServices] TransactionBehavior behavior,
        [FromServices] ApplicationDbContext dbContext,
        [FromServices] SqlKataQueryRunner runner,
        [FromServices] IClock clock,
        HttpContext httpContext,
        Guid proformaId)
    {
        var command = new Command()
        {
        };

        await Handle(behavior, dbContext, proformaId, clock, command);

        httpContext.Response.Headers.TriggerShowSuccessMessage($"proforma", "opened", proformaId);

        return await GetProforma.HandlePage(runner, dbContext, proformaId);
    }
}
