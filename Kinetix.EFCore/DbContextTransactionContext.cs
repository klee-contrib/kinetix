using Kinetix.Services;
using Microsoft.EntityFrameworkCore;

namespace Kinetix.EFCore;

internal class DbContextTransactionContext(DbContext dbContext) : IAsyncTransactionContext, ISyncTransactionContext
{
    /// <inheritdoc />
    public bool Completed { get; set; }

    /// <inheritdoc />
    public TransactionContextStatus Status { get; set; }

    /// <inheritdoc cref="ISyncTransactionContext.Init" />
    public void Init()
    {
        dbContext.Database.BeginTransaction();
    }

    /// <inheritdoc cref="IAsyncTransactionContext.Init" />
    public async Task Init(CancellationToken ct = default)
    {
        await dbContext.Database.BeginTransactionAsync(ct);
    }

    /// <inheritdoc cref="ISyncTransactionContext.OnAfterCommit" />
    public void OnAfterCommit() { }

    /// <inheritdoc cref="IAsyncTransactionContext.OnAfterCommit" />
    public Task OnAfterCommit(CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc cref="ISyncTransactionContext.OnBeforeCommit" />
    public void OnBeforeCommit() { }

    /// <inheritdoc cref="IAsyncTransactionContext.OnBeforeCommit" />
    public Task OnBeforeCommit(CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc cref="ISyncTransactionContext.OnCommit" />
    public void OnCommit()
    {
        if (Completed)
        {
            dbContext.Database.CommitTransaction();
        }
        else
        {
            dbContext.Database.RollbackTransaction();
        }
    }

    /// <inheritdoc cref="IAsyncTransactionContext.OnCommit" />
    public async Task OnCommit(CancellationToken ct = default)
    {
        if (Completed)
        {
            await dbContext.Database.CommitTransactionAsync(ct);
        }
        else
        {
            await dbContext.Database.RollbackTransactionAsync(ct);
        }
    }
}
