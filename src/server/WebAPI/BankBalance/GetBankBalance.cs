using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.SqlKata;

namespace WebAPI.BankBalance;

public static class GetBankBalance
{
    public class Query
    {
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public string Currency { get; set; } = default!;
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
                .Query(Tables.VwBankBalance)
                .SelectRaw("SUM(Total*Sign-ITF) as Total")
                .Where(Tables.VwBankBalance.Field("Currency"), query.Currency);

                if (query.End.HasValue && query.Start.HasValue)
                {
                    statement = statement.WhereBetween(Tables.VwBankBalance.Field("IssuedAt"), query.Start, query.End);
                }
                else
                {
                    if (query.Start.HasValue)
                    {
                        statement = statement.Where(Tables.VwBankBalance.Field("IssuedAt"), ">=", query.Start);
                    }

                    if (query.End.HasValue)
                    {
                        statement = statement.Where(Tables.VwBankBalance.Field("IssuedAt"), "<=", query.End);
                    }
                }

                return statement;
            });
        }
    }
}
