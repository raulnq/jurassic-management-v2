using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.Ui;
using WebAPI.Proformas;

namespace WebAPI.Transactions;

public static class EditTransaction
{
    public class Command
    {
        public TransactionType Type { get; set; }
        public Currency Currency { get; set; }
        public string Description { get; set; } = default!;
        public decimal SubTotal { get; set; }
        public decimal Taxes { get; set; }
        public DateTime IssuedAt { get; set; }
        public string? Number { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.Description).MaximumLength(1000).NotEmpty();
            RuleFor(command => command.Number).MaximumLength(50);
            RuleFor(command => command.SubTotal).GreaterThan(0);
            RuleFor(command => command.Taxes).GreaterThanOrEqualTo(0);
        }
    }

    public static async Task<Ok> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext dbContext,
    [FromRoute] Guid transactionId,
    [FromBody] Command command)
    {
        new Validator().ValidateAndThrow(command);

        await behavior.Handle(async () =>
        {
            var transaction = await dbContext.Get<Transaction>(transactionId);

            transaction.Edit(command.Type,
                command.Description,
                command.SubTotal,
                command.Taxes,
                command.Currency,
                command.Number,
                command.IssuedAt);
        });

        return TypedResults.Ok();
    }

    public static async Task<RazorComponentResult> HandlePage(
    [FromServices] ApplicationDbContext dbContext,
    [FromRoute] Guid transactionId)
    {
        var transaction = await dbContext.Set<Transaction>().AsNoTracking().FirstAsync(t => t.TransactionId == transactionId);

        return new RazorComponentResult<EditTransactionPage>(new { Transaction = transaction });
    }

    public static async Task<RazorComponentResult> HandleAction(
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext dbContext,
    [FromRoute] Guid transactionId,
    [FromBody] Command command,
    HttpContext context)
    {
        await Handle(behavior, dbContext, transactionId, command);

        context.Response.Headers.TriggerShowEditSuccessMessage($"transaction", transactionId);

        return await HandlePage(dbContext, transactionId);
    }
}