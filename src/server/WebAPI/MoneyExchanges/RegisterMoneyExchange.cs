using FluentValidation;
using Infrastructure;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Infrastructure.Ui;
using WebAPI.Proformas;

namespace WebAPI.MoneyExchanges;

public static class RegisterMoneyExchange
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

    public class Result
    {
        public Guid MoneyExchangeId { get; set; }
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

    public static async Task<Ok<Result>> Handle(
    [FromServices] TransactionBehavior behavior,
    [FromServices] ApplicationDbContext dbContext,
    [FromServices] IClock clock,
    [FromBody] Command command)
    {
        new Validator().ValidateAndThrow(command);

        var result = await behavior.Handle(() =>
        {
            var moneyExchange = new MoneyExchange(NewId.Next().ToSequentialGuid(),
                command.FromCurrency, command.FromAmount, command.ToCurrency, command.ToAmount, command.Rate, command.IssuedAt, clock.Now);

            dbContext.Set<MoneyExchange>().Add(moneyExchange);

            return Task.FromResult(new Result()
            {
                MoneyExchangeId = moneyExchange.MoneyExchangeId
            });
        });

        return TypedResults.Ok(result);
    }

    public static Task<RazorComponentResult> HandlePage()
    {
        return Task.FromResult<RazorComponentResult>(new RazorComponentResult<RegisterMoneyExchangePage>(new { }));
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

        context.Response.Headers.TriggerShowRegisterSuccessMessage($"money exchange", result.Value!.MoneyExchangeId);

        return await ListMoneyExchanges.HandlePage(new ListMoneyExchanges.Query() { }, runner);
    }
}
