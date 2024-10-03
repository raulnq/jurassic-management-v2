using FluentValidation;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Proformas;


namespace WebAPI.Invoices;

public static class RegisterInvoice
{
    public class Command
    {
        public Guid ClientId { get; set; }
        public Currency Currency { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Taxes { get; set; }
        public Guid InvoiceId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }

    public class Result
    {
        public Guid InvoiceId { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.SubTotal).GreaterThan(0);
            RuleFor(command => command.Taxes).GreaterThanOrEqualTo(0);
            RuleFor(command => command.InvoiceId).NotEmpty();
        }
    }

    public class Handler
    {
        private readonly ApplicationDbContext _context;

        public Handler(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<Result> Handle(Command command)
        {
            var invoice = new Invoice(command.InvoiceId, command.ClientId, command.SubTotal, command.Taxes, command.Currency, command.CreatedAt);

            _context.Set<Invoice>().Add(invoice);

            return Task.FromResult(new Result()
            {
                InvoiceId = invoice.InvoiceId
            });
        }
    }
}
