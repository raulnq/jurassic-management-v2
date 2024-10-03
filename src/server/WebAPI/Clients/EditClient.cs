using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.ClientContacts;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Infrastructure.Ui;
using WebAPI.Projects;

namespace WebAPI.Clients;

public static class EditClient
{
    public class Command
    {
        public string Name { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public string DocumentNumber { get; set; } = default!;
        public string Address { get; set; } = default!;
        public decimal PenaltyMinimumHours { get; set; }
        public decimal PenaltyAmount { get; set; }
        public decimal TaxesExpensesPercentage { get; set; }
        public decimal AdministrativeExpensesPercentage { get; set; }
        public decimal BankingExpensesPercentage { get; set; }
        public decimal MinimumBankingExpenses { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.Name).MaximumLength(200).NotEmpty();
            RuleFor(command => command.PhoneNumber).MaximumLength(50).NotEmpty();
            RuleFor(command => command.DocumentNumber).MaximumLength(50).NotEmpty();
            RuleFor(command => command.Address).MaximumLength(500).NotEmpty();
            RuleFor(command => command.PenaltyMinimumHours).GreaterThanOrEqualTo(0);
            RuleFor(command => command.PenaltyAmount).GreaterThanOrEqualTo(0);
            RuleFor(command => command.TaxesExpensesPercentage).GreaterThanOrEqualTo(0).LessThanOrEqualTo(100);
            RuleFor(command => command.AdministrativeExpensesPercentage).GreaterThanOrEqualTo(0).LessThanOrEqualTo(100);
            RuleFor(command => command.BankingExpensesPercentage).GreaterThanOrEqualTo(0).LessThanOrEqualTo(100);
            RuleFor(command => command.MinimumBankingExpenses).GreaterThanOrEqualTo(0);
        }
    }

    public static async Task<Ok> Handle(
        [FromServices] TransactionBehavior behavior,
        [FromServices] ApplicationDbContext dbContext,
        [FromRoute] Guid clientId,
        [FromBody] Command command)
    {
        new Validator().ValidateAndThrow(command);

        await behavior.Handle(async () =>
        {
            var client = await dbContext.Get<Client>(clientId);

            client.Edit(command.Name!, command.PhoneNumber, command.DocumentNumber, command.Address);

            client.EditExpenses(command.TaxesExpensesPercentage, command.AdministrativeExpensesPercentage, command.BankingExpensesPercentage, command.MinimumBankingExpenses);

            client.EditPenalty(command.PenaltyMinimumHours, command.PenaltyAmount);
        });

        return TypedResults.Ok();
    }

    public static async Task<RazorComponentResult> HandlePage(
        [FromServices] ApplicationDbContext dbContext,
        [FromServices] SqlKataQueryRunner runner,
        [FromRoute] Guid clientId)
    {
        var client = await dbContext.Set<Client>().AsNoTracking().FirstAsync(c => c.ClientId == clientId);

        var listProjectQuery = new ListProjects.Query() { ClientId = clientId };

        var listProjectResult = await new ListProjects.Runner(runner).Run(listProjectQuery);

        var listClientContactsQuery = new ListClientContacts.Query() { ClientId = clientId };

        var listClientContactsResult = await new ListClientContacts.Runner(runner).Run(listClientContactsQuery);

        return new RazorComponentResult<EditClientPage>(new
        {
            Client = client,
            ListProjectsResult = listProjectResult,
            ListProjectsQuery = listProjectQuery,
            ListClientContactsResult = listClientContactsResult,
            ListClientContactsQuery = listClientContactsQuery
        });
    }

    public static async Task<RazorComponentResult> HandleAction(
        [FromServices] TransactionBehavior behavior,
        [FromServices] ApplicationDbContext dbContext,
        [FromServices] SqlKataQueryRunner runner,
        [FromRoute] Guid clientId,
        [FromBody] Command command,
        HttpContext context)
    {
        await Handle(behavior, dbContext, clientId, command);

        context.Response.Headers.TriggerShowEditSuccessMessage("client", clientId);

        return await HandlePage(dbContext, runner, clientId);
    }
}