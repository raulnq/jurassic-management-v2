using WebAPI.Collaborators;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;

namespace WebAPI.CollaboratorPayments;

public static class GetCollaboratorPayment
{
    public class Query
    {
        public Guid CollaboratorPaymentId { get; set; }
    }

    public class Result
    {
        public Guid CollaboratorPaymentId { get; set; }
        public Guid CollaboratorId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public decimal GrossSalary { get; set; }
        public decimal NetSalary { get; set; }
        public decimal Withholding { get; set; }
        public string? Status { get; set; }
        public string? DocumentUrl { get; set; }
        public string? Number { get; set; }
        public string? CollaboratorName { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public decimal ITF { get; set; }
        public string? Currency { get; set; }
        public DateTimeOffset? CanceledAt { get; set; }
        public decimal ExchangeRate { get; set; }
        public bool ExcludeFromBankBalance { get; set; }
    }

    public class Runner : BaseRunner
    {
        public Runner(SqlKataQueryRunner queryRunner) : base(queryRunner) { }

        public Task<Result> Run(Query query)
        {
            return _queryRunner.Get<Result>((qf) => qf
                .Query(Tables.CollaboratorPayments)
                .Select(Tables.CollaboratorPayments.AllFields)
                .Select(Tables.Collaborators.Field(nameof(Collaborator.Name), nameof(Result.CollaboratorName)))
                .Join(Tables.Collaborators, Tables.CollaboratorPayments.Field(nameof(CollaboratorPayment.CollaboratorId)), Tables.Collaborators.Field(nameof(Collaborator.CollaboratorId)))
                .Where(Tables.CollaboratorPayments.Field(nameof(CollaboratorPayment.CollaboratorPaymentId)), query.CollaboratorPaymentId));
        }
    }
}
