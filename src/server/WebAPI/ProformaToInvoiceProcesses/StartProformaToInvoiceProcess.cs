using FluentValidation;
using Infrastructure;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Clients;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Infrastructure.Ui;
using WebAPI.Invoices;
using WebAPI.Proformas;

namespace WebAPI.ProformaToInvoiceProcesses;

public static class StartProformaToInvoiceProcess
{
    public class Command
    {
        public IEnumerable<Guid>? ProformaId { get; set; }
        public Guid ClientId { get; set; }
        public Currency Currency { get; set; }
    }

    public class Result
    {
        public Guid InvoiceId { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.ProformaId).NotEmpty();
        }
    }

    public static async Task<Ok<Result>> Handle(
        [FromServices] TransactionBehavior behavior,
        [FromServices] ApplicationDbContext dbContext,
        [FromServices] IClock clock,
        [FromBody] Command command)
    {
        new Validator().ValidateAndThrow(command);

        var proformas = await dbContext.Set<Proforma>().AsNoTracking().Where(p => command.ProformaId!.Contains(p.ProformaId)).ToListAsync();

        var result = await behavior.Handle(async () =>
        {
            var process = new ProformaToInvoiceProcess(NewId.Next().ToSequentialGuid(), command.ClientId, command.Currency, proformas, clock.Now);

            dbContext.Set<ProformaToInvoiceProcess>().Add(process);

            await new RegisterInvoice.Handler(dbContext).Handle(new RegisterInvoice.Command()
            {
                InvoiceId = process.InvoiceId,
                CreatedAt = clock.Now,
                SubTotal = proformas.Sum(p => p.Total),
                Taxes = 0,
                Currency = command.Currency,
                ClientId = command.ClientId,
            });

            return new Result()
            {
                InvoiceId = process.InvoiceId
            };
        });

        return TypedResults.Ok(result);
    }

    public static async Task<RazorComponentResult> HandlePage(
    [FromServices] ApplicationDbContext dbContext)
    {
        var clients = await dbContext.Set<Client>().AsNoTracking().ToListAsync();

        return new RazorComponentResult<StartProformaToInvoiceProcessPage>(new
        {
            Clients = clients
        });
    }

    public static async Task<RazorComponentResult> HandleAction(
        [FromServices] TransactionBehavior behavior,
        [FromServices] SqlKataQueryRunner runner,
        [FromServices] ApplicationDbContext dbContext,
        [FromBody] Command command,
        [FromServices] IClock clock,
        HttpContext httpContext)
    {
        var register = await Handle(behavior, dbContext, clock, command);

        httpContext.Response.Headers.TriggerShowRegisterSuccessMessage($"invoice", register.Value!.InvoiceId);

        return await ListInvoices.HandlePage(new ListInvoices.Query() { }, dbContext, runner);
    }
}
