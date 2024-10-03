namespace Migrator;

public class DbMigrationException : Exception
{
    public DbMigrationException(Exception innerException) : base("Migration failed", innerException)
    {
    }
}