using FluentValidation;
using Infrastructure;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Infrastructure.Ui;
using WebAPI.Proformas;

namespace WebAPI.Transactions;

public static class RegisterTransaction
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

    public class Result
    {
        public Guid TransactionId { get; set; }
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

    public static async Task<Ok<Result>> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext dbContext,
    [FromServices] IClock clock,
    [FromBody] Command command)
    {
        new Validator().ValidateAndThrow(command);

        var result = await behavior.Handle(() =>
        {
            var transaction = new Transaction(NewId.Next().ToSequentialGuid(), command.Type,
    command.Description, command.SubTotal, command.Taxes, command.Currency, command.Number, command.IssuedAt, clock.Now);

            dbContext.Set<Transaction>().Add(transaction);

            return Task.FromResult(new Result()
            {
                TransactionId = transaction.TransactionId
            });
        });

        return TypedResults.Ok(result);
    }

    public static Task<RazorComponentResult> HandlePage()
    {
        return Task.FromResult<RazorComponentResult>(new RazorComponentResult<RegisterTransactionPage>(new { }));
    }

    public static async Task<RazorComponentResult> HandleAction(
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext dbContext,
    [FromServices] SqlKataQueryRunner runner,
    [FromBody] Command command,
    [FromServices] IClock clock,
    HttpContext context)
    {
        var result = await Handle(behavior, dbContext, clock, command);

        context.Response.Headers.TriggerShowRegisterSuccessMessage($"transaction", result.Value!.TransactionId);

        return await ListTransactions.HandlePage(new ListTransactions.Query() { }, runner);
    }
}
