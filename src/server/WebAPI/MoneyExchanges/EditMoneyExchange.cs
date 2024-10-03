using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.Ui;
using WebAPI.Proformas;

namespace WebAPI.MoneyExchanges;

public static class EditMoneyExchange
{
    public class Command
    {
        public Currency FromCurrency { get; set; }
        public decimal FromAmount { get; set; }
        public Currency ToCurrency { get; set; }
        public decimal ToAmount { get; set; }
        public decimal Rate { get; set; }
        public DateTime IssuedAt { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.FromAmount).GreaterThan(0);
            RuleFor(command => command.ToAmount).GreaterThan(0);
            RuleFor(command => command.Rate).GreaterThan(0);
        }
    }

    public static async Task<Ok> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext dbContext,
    [FromRoute] Guid moneyExchangeId,
    [FromBody] Command command)
    {
        new Validator().ValidateAndThrow(command);

        await behavior.Handle(async () =>
        {
            var moneyExchange = await dbContext.Get<MoneyExchange>(moneyExchangeId);

            moneyExchange.Edit(command.FromCurrency,
                command.FromAmount,
                command.ToCurrency,
                command.ToAmount,
                command.Rate,
                command.IssuedAt);
        });

        return TypedResults.Ok();
    }

    public static async Task<RazorComponentResult> HandlePage(
    [FromServices] ApplicationDbContext dbContext,
    [FromRoute] Guid moneyExchangeId)
    {
        var moneyExchange = await dbContext.Set<MoneyExchange>().AsNoTracking().FirstAsync(t => t.MoneyExchangeId == moneyExchangeId);

        return new RazorComponentResult<EditMoneyExchangePage>(new { MoneyExchange = moneyExchange });
    }

    public static async Task<RazorComponentResult> HandleAction(
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext dbContext,
    [FromRoute] Guid moneyExchangeId,
    [FromBody] Command command,
    HttpContext context)
    {
        await Handle(behavior, dbContext, moneyExchangeId, command);

        context.Response.Headers.TriggerShowEditSuccessMessage($"money exchange", moneyExchangeId);

        return await HandlePage(dbContext, moneyExchangeId);
    }
}