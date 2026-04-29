using System.Data.Common;
using System.Transactions;
using Kinetix.Services;

namespace Kinetix.DataAccess.Sql;

/// <summary>
/// Contexte de transaction pour les connections en BDD.
/// </summary>
internal class SqlTransactionContext : ISyncTransactionContext
{
    private TransactionScope _scope;

    /// <inheritdoc />
    public bool Completed { get; set; }

    /// <inheritdoc />
    public TransactionContextStatus Status { get; set; }

    /// <summary>
    /// Connections.
    /// </summary>
    internal Dictionary<string, DbConnection> Connections { get; } = [];

    /// <inheritdoc cref="ISyncTransactionContext.Init" />
    public void Init()
    {
        _scope = new(
            TransactionScopeOption.RequiresNew,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted, Timeout = TimeSpan.Zero }
        );
    }

    /// <inheritdoc cref="ISyncTransactionContext.OnAfterCommit" />
    public void OnAfterCommit() { }

    /// <inheritdoc cref="ISyncTransactionContext.OnBeforeCommit" />
    public void OnBeforeCommit() { }

    /// <inheritdoc cref="ISyncTransactionContext.OnCommit" />
    public void OnCommit()
    {
        foreach (var connection in Connections)
        {
            connection.Value.Dispose();
        }

        if (Completed)
        {
            _scope?.Complete();
        }

        _scope?.Dispose();
    }
}
