using System.Data;
using System.Transactions;
using Kinetix.Services;

namespace Kinetix.DataAccess.Sql;

/// <summary>
/// Contexte de transaction pour les connections en BDD.
/// </summary>
internal class SqlTransactionContext : ITransactionContext
{
    private readonly TransactionScope _scope = new(TransactionScopeOption.RequiresNew, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted, Timeout = TimeSpan.Zero });

    /// <inheritdoc />
    public bool Completed { get; set; }

    /// <summary>
    /// Connections.
    /// </summary>
    internal Dictionary<string, IDbConnection> Connections { get; } = new();

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
        foreach (var connection in Connections)
        {
            connection.Value.Dispose();
        }

        if (Completed)
        {
            _scope.Complete();
        }

        _scope.Dispose();
    }
}
