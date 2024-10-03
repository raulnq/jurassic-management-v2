using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Infrastructure.Ui;

namespace WebAPI.ClientContacts;

public static class EditClientContact
{
    public class Command
    {
        public string Name { get; set; } = default!;
        public string Email { get; set; } = default!;
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.Name).MaximumLength(100).NotEmpty();
            RuleFor(command => command.Email).MaximumLength(255).EmailAddress();
        }
    }

    public static async Task<Ok> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext dbContext,
    [FromRoute] Guid clientContactId,
    [FromRoute] Guid clientId,
    [FromBody] Command command)
    {
        new Validator().ValidateAndThrow(command);

        await behavior.Handle(async () =>
        {
            var contact = await dbContext.Get<ClientContact>(clientContactId);

            contact.Edit(command.Name!, command.Email!);
        });

        return TypedResults.Ok();
    }

    public static async Task<RazorComponentResult> HandlePage(
        Guid clientContactId,
        Guid clientId,
        ApplicationDbContext dbContext,
        HttpContext context)
    {
        var contact = await dbContext.Set<ClientContact>().AsNoTracking().FirstAsync(c => c.ClientContactId == clientContactId);

        context.Response.Headers.TriggerOpenModal();

        return new RazorComponentResult<EditClientContactPage>(new { Contact = contact });
    }

    public static async Task<RazorComponentResult> HandleAction(
        [FromServices] TransactionBehavior behavior,
        [FromServices] ApplicationDbContext dbContext,
        [FromServices] SqlKataQueryRunner runner,
        [FromBody] Command command,
        Guid clientId,
        Guid clientContactId,
        HttpContext context)
    {
        await Handle(behavior, dbContext, clientContactId, clientId, command);

        context.Response.Headers.TriggerShowEditSuccessMessageAndCloseModal("contact", clientContactId);

        return await ListClientContacts.HandlePage(new ListClientContacts.Query() { ClientId = clientId }, clientId, runner);
    }
}