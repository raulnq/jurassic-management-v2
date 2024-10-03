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
using WebAPI.Proformas;


namespace WebAPI.CollaboratorPayments;

public static class RegisterCollaboratorPayment
{
    public class Command
    {
        public decimal GrossSalary { get; set; }
        [JsonIgnore]
        public Guid CollaboratorPaymentId { get; set; }
        public Guid CollaboratorId { get; set; }
        [JsonIgnore]
        public DateTimeOffset CreatedAt { get; set; }
        public Currency Currency { get; set; }
    }

    public class Result
    {
        public Guid CollaboratorPaymentId { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.GrossSalary).GreaterThan(0);
            RuleFor(command => command.CollaboratorPaymentId).NotEmpty();
            RuleFor(command => command.CollaboratorId).NotEmpty();
        }
    }

    public class Handler
    {
        private readonly ApplicationDbContext _context;

        public Handler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(Command command)
        {
            var collaborator = await _context.Set<Collaborator>().AsNoTracking().FirstAsync(cr => cr.CollaboratorId == command.CollaboratorId);

            var payment = new CollaboratorPayment(command.CollaboratorPaymentId, command.CollaboratorId, command.GrossSalary, collaborator.WithholdingPercentage, command.Currency, command.CreatedAt);

            _context.Set<CollaboratorPayment>().Add(payment);

            return new Result()
            {
                CollaboratorPaymentId = payment.CollaboratorPaymentId
            };
        }
    }

    public static async Task<Ok<Result>> Handle(
        [FromServices] TransactionBehavior behavior,
        [FromServices] ApplicationDbContext dbContext,
        [FromServices] IClock clock,
        [FromBody] Command command)
    {
        command.CreatedAt = clock.Now;

        command.CollaboratorPaymentId = NewId.Next().ToSequentialGuid();

        new Validator().ValidateAndThrow(command);

        var result = await behavior.Handle(() => new Handler(dbContext).Handle(command));

        return TypedResults.Ok(result);
    }

    public static async Task<RazorComponentResult> HandlePage(
        [FromServices] ApplicationDbContext dbContext)
    {
        var collaborators = await dbContext.Set<Collaborator>().AsNoTracking().ToListAsync();

        return new RazorComponentResult<RegisterCollaboratorPaymentPage>(new
        {
            Collaborators = collaborators
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

        context.Response.Headers.TriggerShowRegisterSuccessMessage("collaborator payment", result.Value!.CollaboratorPaymentId);

        return await ListCollaboratorPayments.HandlePage(new ListCollaboratorPayments.Query(), runner);
    }
}
