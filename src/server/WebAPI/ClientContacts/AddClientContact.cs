using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Infrastructure.Ui;

namespace WebAPI.ClientContacts;

public static class AddClientContact
{
    public class Command
    {
        public string Name { get; set; } = default!;
        public string Email { get; set; } = default!;
    }

    public class Result
    {
        public Guid ClientContactId { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.Name).MaximumLength(100).NotEmpty();
            RuleFor(command => command.Email).MaximumLength(255).EmailAddress();
        }
    }

    public static async Task<Ok<Result>> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext dbContext,
    [FromRoute] Guid clientId,
    [FromBody] Command command)
    {
        new Validator().ValidateAndThrow(command);

        var result = await behavior.Handle(() =>
        {
            var contact = new ClientContact(NewId.Next().ToSequentialGuid(), clientId, command.Name!, command.Email);

            dbContext.Set<ClientContact>().Add(contact);

            return Task.FromResult(new Result()
            {
                ClientContactId = contact.ClientContactId
            });
        });

        return TypedResults.Ok(result);
    }

    public static Task<RazorComponentResult> HandlePage(Guid clientId, HttpContext context)
    {
        context.Response.Headers.TriggerOpenModal();

        return Task.FromResult<RazorComponentResult>(new RazorComponentResult<AddClientContactPage>(new { ClientId = clientId }));
    }

    public static async Task<RazorComponentResult> HandleAction(
        [FromServices] TransactionBehavior behavior,
        [FromServices] ApplicationDbContext dbContext,
        [FromServices] SqlKataQueryRunner runner,
        [FromBody] Command command,
        Guid clientId,
        HttpContext context)
    {
        var result = await Handle(behavior, dbContext, clientId, command);

        context.Response.Headers.TriggerShowRegisterSuccessMessageAndCloseModal("contact", result.Value!.ClientContactId);

        return await ListClientContacts.HandlePage(new ListClientContacts.Query() { ClientId = clientId }, clientId, runner);
    }

}
