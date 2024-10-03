namespace WebAPI.Infrastructure.ExceptionHandling;

public abstract class BaseException : Exception
{
    protected BaseException(string code, object[] parameters)
        : base($"code: {code}, parameters: {string.Join(",", parameters)}")
    {
    }

    protected BaseException(string code, string description)
        : base($"code: {code}, description: {description}")
    {
    }

    protected BaseException(string code) : base($"code: {code}")
    {
    }
}

public class NotFoundException : BaseException
{
    public NotFoundException() : base("resource-not-found")
    {
    }

    public NotFoundException(string code) : base(code)
    {
    }
}

public class NotFoundException<TEntity> : NotFoundException
{
    public NotFoundException() : base($"{typeof(TEntity).Name.ToLower()}-not-found")
    {
    }
}

public class DomainException : BaseException
{
    public DomainException(string code, params object[] parameters) : base(code, parameters)
    {
    }

    public DomainException(string code, string description) : base(code, description)
    {
    }

    public DomainException(string code) : base(code)
    {
    }
}

public class InfrastructureException : BaseException
{
    public InfrastructureException(string code, params object[] parameters) : base(code, parameters)
    {
    }

    public InfrastructureException(string code, string description) : base(code, description)
    {
    }

    public InfrastructureException(string code) : base(code)
    {
    }
}

public static class ExceptionCodes
{
    public const string Duplicated = "the-resource-is-duplicated";
}
