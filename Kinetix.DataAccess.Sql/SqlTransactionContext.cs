using System.Data.Common;
using Kinetix.Services;

namespace Kinetix.DataAccess.Sql;

/// <summary>
/// Contexte de transaction pour les connections en BDD.
/// </summary>
internal class SqlTransactionContext : ISyncTransactionContext, IAsyncTransactionContext
{
    /// <inheritdoc />
    public bool Completed { get; set; }

    /// <inheritdoc />
    public TransactionContextStatus Status { get; set; }

    /// <summary>
    /// Connections.
    /// </summary>
    internal Dictionary<string, (DbConnection Connection, DbTransaction Transaction)> Connections { get; } = [];

    /// <inheritdoc cref="ISyncTransactionContext.Init" />
    public void Init() { }

    /// <inheritdoc cref="IAsyncTransactionContext.InitAsync" />
    public Task InitAsync(CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc cref="ISyncTransactionContext.OnAfterCommit" />
    public void OnAfterCommit() { }

    /// <inheritdoc cref="IAsyncTransactionContext.OnAfterCommitAsync" />
    public Task OnAfterCommitAsync(CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc cref="ISyncTransactionContext.OnBeforeCommit" />
    public void OnBeforeCommit() { }

    /// <inheritdoc cref="IAsyncTransactionContext.OnBeforeCommitAsync" />
    public Task OnBeforeCommitAsync(CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc cref="ISyncTransactionContext.OnCommit" />
    public void OnCommit()
    {
        foreach (var connection in Connections)
        {
            using var dbConnection = connection.Value.Connection;
            using var transaction = connection.Value.Transaction;

            if (Completed)
            {
                transaction.Commit();
            }
            else
            {
                transaction.Rollback();
            }
        }

        Connections.Clear();
    }

    /// <inheritdoc cref="IAsyncTransactionContext.OnCommitAsync" />
    public async Task OnCommitAsync(CancellationToken ct = default)
    {
        foreach (var connection in Connections)
        {
            await using var dbConnection = connection.Value.Connection;
            await using var transaction = connection.Value.Transaction;

            if (Completed)
            {
                await transaction.CommitAsync(ct);
            }
            else
            {
                await transaction.RollbackAsync(ct);
            }
        }

        Connections.Clear();
    }
}
