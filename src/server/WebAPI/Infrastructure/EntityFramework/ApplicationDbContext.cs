using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Reflection;
using System.Data;
using WebAPI.Infrastructure.ExceptionHandling;

namespace WebAPI.Infrastructure.EntityFramework;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }

    public IDbContextTransaction? CurrentTransaction { get; private set; }

    public async Task BeginTransaction()
    {
        if (CurrentTransaction != null)
        {
            return;
        }

        CurrentTransaction = await base.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
    }

    public async Task Rollback()
    {
        if (CurrentTransaction == null)
        {
            return;
        }

        try
        {
            await CurrentTransaction.RollbackAsync();
        }
        finally
        {
            if (CurrentTransaction != null)
            {
                CurrentTransaction.Dispose();
                CurrentTransaction = null;
            }
        }
    }

    public async Task Commit()
    {
        if (CurrentTransaction == null)
        {
            return;
        }
        try
        {
            await SaveChangesAsync();

            await CurrentTransaction.CommitAsync();
        }
        catch
        {
            await Rollback();
            throw;
        }
        finally
        {
            if (CurrentTransaction != null)
            {
                CurrentTransaction.Dispose();
                CurrentTransaction = null;
            }
        }
    }

    public bool IsTransactionOpened()
    {
        return CurrentTransaction != null;
    }

    public async Task<T> Get<T>(params object[] keyValues) where T : class
    {
        var entity = await Set<T>().FindAsync(keyValues);

        if (entity == null)
        {
            throw new NotFoundException<T>();
        }

        return entity;
    }
}