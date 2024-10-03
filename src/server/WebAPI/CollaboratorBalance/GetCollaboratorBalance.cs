using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;

namespace WebAPI.CollaboratorBalance;

public static class GetCollaboratorBalance
{
    public class Query
    {
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public string Currency { get; set; } = default!;
        public Guid CollaboratorId { get; set; }
    }

    public class Result
    {
        public decimal Total { get; set; }
    }

    public class Runner : BaseRunner
    {
        public Runner(SqlKataQueryRunner queryRunner) : base(queryRunner) { }

        public Task<Result> Run(Query query)
        {
            return _queryRunner.Get<Result>(qf =>
            {
                var statement = qf
                .Query(Tables.VwCollaboratorBalance)
                .SelectRaw("SUM(NetSalary*Sign) as Total")
                .Where(Tables.VwCollaboratorBalance.Field("Currency"), query.Currency)
                .Where(Tables.VwCollaboratorBalance.Field("CollaboratorId"), query.CollaboratorId);

                if (query.End.HasValue && query.Start.HasValue)
                {
                    statement = statement.WhereBetween(Tables.VwCollaboratorBalance.Field("Date"), query.Start, query.End);
                }
                else
                {
                    if (query.Start.HasValue)
                    {
                        statement = statement.Where(Tables.VwCollaboratorBalance.Field("Date"), ">=", query.Start);
                    }

                    if (query.End.HasValue)
                    {
                        statement = statement.Where(Tables.VwCollaboratorBalance.Field("Date"), "<=", query.End);
                    }
                }

                return statement;
            });
        }
    }
}
