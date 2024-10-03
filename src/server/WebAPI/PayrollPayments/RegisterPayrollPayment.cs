using FluentValidation;
using Infrastructure;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using WebAPI.Collaborators;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Infrastructure.Ui;
using WebAPI.MoneyExchanges;
using WebAPI.Proformas;


namespace WebAPI.PayrollPayments;

public static class RegisterPayrollPayment
{
    public class Command
    {
        public decimal NetSalary { get; set; }
        public decimal Afp { get; set; }
        public decimal Commission { get; set; }
        [JsonIgnore]
        public Guid PayrollPaymentId { get; set; }
        public Guid CollaboratorId { get; set; }
        [JsonIgnore]
        public DateTimeOffset CreatedAt { get; set; }
        public Currency Currency { get; set; }
        public Guid MoneyExchangeId { get; set; }
    }

    public class Result
    {
        public Guid PayrollPaymentId { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.NetSalary).GreaterThan(0);
            RuleFor(command => command.Afp).GreaterThanOrEqualTo(0);
            RuleFor(command => command.Commission).GreaterThanOrEqualTo(0);
            RuleFor(command => command.PayrollPaymentId).NotEmpty();
            RuleFor(command => command.CollaboratorId).NotEmpty();
        }
    }

    public static async Task<Ok<Result>> Handle(
        [FromServices] TransactionBehavior behavior,
        [FromServices] ApplicationDbContext dbContext,
        [FromServices] IClock clock,
        [FromBody] Command command)
    {
        command.CreatedAt = clock.Now;

        command.PayrollPaymentId = NewId.Next().ToSequentialGuid();

        new Validator().ValidateAndThrow(command);

        var result = await behavior.Handle(async () =>
        {
            var collaborator = await dbContext.Set<Collaborator>().AsNoTracking().FirstAsync(cr => cr.CollaboratorId == command.CollaboratorId);

            var payment = new PayrollPayment(command.PayrollPaymentId, command.CollaboratorId, command.NetSalary, command.Afp, command.Commission, command.Currency, command.CreatedAt, command.MoneyExchangeId);

            dbContext.Set<PayrollPayment>().Add(payment);

            return new Result()
            {
                PayrollPaymentId = payment.PayrollPaymentId
            };
        });

        return TypedResults.Ok(result);
    }

    public static async Task<RazorComponentResult> HandlePage(
        [FromServices] ApplicationDbContext dbContext)
    {
        var collaborators = await dbContext.Set<Collaborator>().AsNoTracking().ToListAsync();

        var moneyExchanges = await dbContext.Set<MoneyExchange>().AsNoTracking().ToListAsync();

        return new RazorComponentResult<RegisterPayrollPaymentPage>(new
        {
            Collaborators = collaborators,
            MoneyExchanges = moneyExchanges
        });
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

        context.Response.Headers.TriggerShowRegisterSuccessMessage("payroll payment", result.Value!.PayrollPaymentId);

        return await ListPayrollPayments.HandlePage(new ListPayrollPayments.Query(), runner);
    }
}
