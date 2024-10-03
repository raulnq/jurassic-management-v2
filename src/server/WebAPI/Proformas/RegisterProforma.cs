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
using WebAPI.Projects;

namespace WebAPI.Proformas;

public static class RegisterProforma
{
    public class Command
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public Guid ProjectId { get; set; }
        public decimal Discount { get; set; }
        public string Note { get; set; } = string.Empty;
        public Currency Currency { get; set; }
    }

    public class Result
    {
        public Guid ProformaId { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.ProjectId).NotEmpty();
            RuleFor(command => command.Note).NotEmpty().MaximumLength(1000);
            RuleFor(command => command.End).GreaterThanOrEqualTo(command => command.Start);
        }
    }

    public static async Task<Ok<Result>> Handle(
        [FromServices] TransactionBehavior behavior,
        [FromServices] ApplicationDbContext dbContext,
        [FromServices] IClock clock,
        [FromBody] Command command)
    {
        new Validator().ValidateAndThrow(command);

        var result = await behavior.Handle(async () =>
        {
            var project = await dbContext.Get<Project>(command.ProjectId);

            var client = await dbContext.Get<Client>(project.ClientId);

            var count = await dbContext.Set<Proforma>().AsNoTracking().CountAsync(p => p.End == command.End);

            var proforma = new Proforma(NewId.Next().ToSequentialGuid(), command.Start, command.End, command.ProjectId, client, clock.Now, command.Discount, command.Currency, count, command.Note);

            dbContext.Set<Proforma>().Add(proforma);

            return new Result()
            {
                ProformaId = proforma.ProformaId
            };
        });

        return TypedResults.Ok(result);
    }

    public static async Task<RazorComponentResult> HandlePage(
        [FromServices] ApplicationDbContext dbContext)
    {
        var clients = await dbContext.Set<Client>().AsNoTracking().ToListAsync();

        return new RazorComponentResult<RegisterProformaPage>(new
        {
            Clients = clients
        });
    }

    public static async Task<RazorComponentResult> HandleAction(
        [FromServices] TransactionBehavior behavior,
        [FromServices] ApplicationDbContext dbContext,
        [FromServices] SqlKataQueryRunner runner,
        [FromBody] Command command,
        [FromServices] IClock clock,
        HttpContext context)
    {
        var register = await Handle(behavior, dbContext, clock, command);

        context.Response.Headers.TriggerShowRegisterSuccessMessage($"proforma", register.Value!.ProformaId);

        return await ListProformas.HandlePage(new ListProformas.Query() { }, runner);
    }
}
