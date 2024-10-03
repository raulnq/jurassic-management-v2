using FluentValidation;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Proformas;


namespace WebAPI.Collections;

public static class RegisterCollection
{
    public class Command
    {
        public decimal Total { get; set; }
        public Guid ClientId { get; set; }
        public Guid CollectionId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public Currency Currency { get; set; }
    }

    public class Result
    {
        public Guid CollectionId { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.Total).GreaterThan(0);
            RuleFor(command => command.CollectionId).NotEmpty();
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
            var collection = new Collection(command.CollectionId, command.ClientId, command.Total, command.Currency, command.CreatedAt);

            _context.Set<Collection>().Add(collection);

            return Task.FromResult(new Result()
            {
                CollectionId = collection.CollectionId
            });
        }
    }
}
