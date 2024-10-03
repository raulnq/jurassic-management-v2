using FluentValidation;
using QuestPDF.Fluent;
using Rebus.Handlers;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;
using WebAPI.Proformas;
using static WebAPI.Proformas.IssueProforma;

namespace WebAPI.ProformaDocuments;

public static class RegisterProformaDocument
{
    public class Command
    {
        public Guid ProformaId { get; set; }
        public string Url { get; set; } = default!;
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.ProformaId).NotEmpty();
            RuleFor(command => command.Url).NotEmpty();
        }
    }

    public class ProformaIssuedEventDispatcher : IHandleMessages<ProformaIssued>
    {
        private readonly ApplicationDbContext _dbContext;

        private readonly TransactionBehavior _behavior;

        private readonly ProformaDocumentStorage _storage;

        private readonly SqlKataQueryRunner _runner;

        public ProformaIssuedEventDispatcher(ApplicationDbContext dbContext,
            TransactionBehavior behavior,
            ProformaDocumentStorage storage,
            SqlKataQueryRunner runner)
        {
            _dbContext = dbContext;
            _behavior = behavior;
            _storage = storage;
            _runner = runner;
        }

        public async Task Handle(ProformaIssued message)
        {
            var url = string.Empty;

            var getProformaResult = await new GetProforma.Runner(_runner).Run(new GetProforma.Query() { ProformaId = message.ProformaId });

            var listProformaWeekWorkItemsResult = await new ListProformaWeekWorkItems.Runner(_runner).Run(new ListProformaWeekWorkItems.Query() { ProformaId = message.ProformaId, PageSize = 500 });

            using (var stream = new MemoryStream())
            {
                var document = new ProformaPdf(getProformaResult, listProformaWeekWorkItemsResult.Items);

                document.GeneratePdf(stream);

                stream.Position = 0;

                url = await _storage.Upload($"{message.ProformaId}.pdf", stream);
            }

            var command = new Command { ProformaId = message.ProformaId, Url = url };

            new Validator().ValidateAndThrow(command);

            await _behavior.Handle(() =>
            {
                var proformaDocument = new ProformaDocument(command.ProformaId, command.Url);

                _dbContext.Set<ProformaDocument>().Add(proformaDocument);

                return Task.CompletedTask;
            });
        }
    }
}
