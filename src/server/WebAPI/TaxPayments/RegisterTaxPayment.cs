using FluentValidation;
using Infrastructure;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Infrastructure.Ui;

namespace WebAPI.TaxPayments;

public static class RegisterTaxPayment
{
    public class Command
    {
        public string? Month { get; set; }
    }

    public class Result
    {
        public Guid TaxPaymentId { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.Month).NotEmpty();
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
            var period = command.Month!.Split("-");

            var month = period.Last();

            var year = period.First();

            var taxPayment = new TaxPayment(NewId.Next().ToSequentialGuid(),
                clock.Now,
                month,
                int.Parse(year));

            dbContext.Set<TaxPayment>().Add(taxPayment);

            return Task.FromResult(new Result()
            {
                TaxPaymentId = taxPayment.TaxPaymentId
            });
        });

        return TypedResults.Ok(result);
    }

    public static Task<RazorComponentResult> HandlePage()
    {
        return Task.FromResult<RazorComponentResult>(new RazorComponentResult<RegisterTaxPaymentPage>(new { }));
    }

    public static async Task<RazorComponentResult> HandleAction(
        [FromServices] TransactionBehavior behavior,
        [FromServices] ApplicationDbContext dbContext,
        [FromServices] SqlKataQueryRunner runner,
         [FromServices] IClock clock,
        [FromBody] Command command,
        HttpContext context)
    {
        var result = await Handle(behavior, dbContext, clock, command);

        context.Response.Headers.TriggerShowRegisterSuccessMessage("tax payment", result.Value!.TaxPaymentId);

        return await ListTaxPayments.HandlePage(new ListTaxPayments.Query(), clock, runner);
    }
}