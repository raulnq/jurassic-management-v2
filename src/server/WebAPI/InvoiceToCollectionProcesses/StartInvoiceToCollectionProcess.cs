using FluentValidation;
using Infrastructure;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Clients;
using WebAPI.Collections;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Infrastructure.Ui;
using WebAPI.Invoices;
using WebAPI.Proformas;

namespace WebAPI.InvoiceToCollectionProcesses;

public static class StartInvoiceToCollectionProcess
{
    public class Command
    {
        public IEnumerable<Guid>? InvoiceId { get; set; }
        public Guid ClientId { get; set; }
        public Currency Currency { get; set; }
    }

    public class Result
    {
        public Guid CollectionId { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.InvoiceId).NotEmpty();
        }
    }

    public static async Task<Ok<Result>> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext dbContext,
    [FromServices] IClock clock,
    [FromBody] Command command)
    {
        new Validator().ValidateAndThrow(command);

        var invoices = await dbContext.Set<Invoice>().AsNoTracking().Where(i => command.InvoiceId!.Contains(i.InvoiceId)).ToListAsync();

        var result = await behavior.Handle(async () =>
        {
            var process = new InvoiceToCollectionProcess(NewId.Next().ToSequentialGuid(), command.ClientId, command.Currency, invoices, clock.Now);

            dbContext.Set<InvoiceToCollectionProcess>().Add(process);

            await new RegisterCollection.Handler(dbContext).Handle(new RegisterCollection.Command()
            {
                CollectionId = process.CollectionId,
                ClientId = command.ClientId,
                CreatedAt = clock.Now,
                Total = invoices.Sum(i => i.Total),
                Currency = command.Currency
            });

            return new Result()
            {
                CollectionId = process.CollectionId
            };
        });

        return TypedResults.Ok(result);
    }

    public static async Task<RazorComponentResult> HandlePage(
    [FromServices] ApplicationDbContext dbContext)
    {
        var clients = await dbContext.Set<Client>().AsNoTracking().ToListAsync();

        return new RazorComponentResult<StartInvoiceToCollectionProcessPage>(new
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
    HttpContext context)
    {
        var register = await Handle(behavior, dbContext, clock, command);

        context.Response.Headers.TriggerShowRegisterSuccessMessage($"collection", register.Value!.CollectionId);

        return await ListCollections.HandlePage(new ListCollections.Query() { }, dbContext, runner);
    }
}
