using Kinetix.Services;
using Microsoft.EntityFrameworkCore;

namespace Kinetix.EFCore;

internal class DbContextTransactionContext : ITransactionContext
{
    private readonly DbContext _dbContext;

    public DbContextTransactionContext(DbContext dbContext)
    {
        _dbContext = dbContext;
        _dbContext.Database.BeginTransaction();
    }

    /// <inheritdoc />
    public bool Completed { get; set; }

    /// <inheritdoc cref="ITransactionContext.OnAfterCommit" />
    public void OnAfterCommit()
    {
    }

    /// <inheritdoc cref="ITransactionContext.OnBeforeCommit" />
    public void OnBeforeCommit()
    {
    }

    /// <inheritdoc cref="ITransactionContext.OnCommit" />
    public void OnCommit()
    {
        if (Completed)
        {
            _dbContext.Database.CommitTransaction();
        }
        else
        {
            _dbContext.Database.RollbackTransaction();
        }
    }
}
