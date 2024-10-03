namespace WebAPI.Infrastructure.EntityFramework;

public class TransactionBehavior
{
    private readonly ApplicationDbContext _context;

    public TransactionBehavior(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(Func<Task> action)
    {
        if (_context.IsTransactionOpened())
        {
            await action();
        }

        try
        {
            await _context.BeginTransaction();

            await action();

            await _context.Commit();
        }
        catch
        {
            await _context.Rollback();

            throw;
        }
    }

    public async Task<T> Handle<T>(Func<Task<T>> action)
    {

        if (_context.IsTransactionOpened())
        {
            var result = await action();

            return result;
        }

        T response;

        try
        {
            await _context.BeginTransaction();

            response = await action();

            await _context.Commit();

            return response;
        }
        catch
        {
            await _context.Rollback();

            throw;
        }
    }
}