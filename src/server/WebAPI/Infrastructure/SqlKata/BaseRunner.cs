namespace WebAPI.Infrastructure.SqlKata;

public abstract class BaseRunner
{
    protected readonly SqlKataQueryRunner _queryRunner;

    protected BaseRunner(SqlKataQueryRunner queryRunner)
    {
        _queryRunner = queryRunner;
    }
}
