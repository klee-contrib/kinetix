using System.Data;
using System.Data.Common;
using System.Reflection;
using Kinetix.Services;

namespace Kinetix.DataAccess.Sql.Common;

/// <summary>
/// Pool de connections SQL de base.
/// </summary>
public abstract class ConnectionPool(TransactionScopeManager transactionScopeManager)
{
    /// <summary>
    /// Crée une nouvelle commande SQL.
    /// </summary>
    /// <param name="connectionName">Nom de la connection.</param>
    /// <param name="procName">Nom de la procédure stockée.</param>
    /// <returns>Commande SQL.</returns>
    public abstract BaseSqlCommand GetSqlCommand(string connectionName, string procName);

    /// <summary>
    /// Crée une nouvelle commande SQL.
    /// </summary>
    /// <param name="connectionName">Nom de la connection.</param>
    /// <param name="assembly">Assembly dans lequel chercher la requête SQL.</param>
    /// <param name="resourcePath">Chemin vers le fichier SQL.</param>
    /// <returns>Commande SQL.</returns>
    public abstract BaseSqlCommand GetSqlCommand(string connectionName, Assembly assembly, string resourcePath);

    /// <summary>
    /// Crée une nouvelle commande SQL.
    /// </summary>
    /// <param name="connectionName">Nom de la connection.</param>
    /// <param name="commandName">Nom de la commande.</param>
    /// <param name="commandText">Requête SQL.</param>
    /// <returns>Commande SQL.</returns>
    public abstract BaseSqlCommand GetSqlCommand(string connectionName, string commandName, string commandText);

    /// <summary>
    /// Crée une nouvelle commande SQL.
    /// </summary>
    /// <param name="connectionName">Nom de la connection.</param>
    /// <param name="commandName">Nom de la commande.</param>
    /// <param name="commandType">Type de la commande.</param>
    /// <returns>Commande SQL.</returns>
    public abstract BaseSqlCommand GetSqlCommand(string connectionName, string commandName, CommandType commandType);

    /// <summary>
    /// Crée une nouvelle commande SQL.
    /// </summary>
    /// <param name="connectionName">Nom de la connection.</param>
    /// <param name="procName">Nom de la procédure stockée.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Commande SQL.</returns>
    public abstract Task<BaseSqlCommand> GetSqlCommandAsync(
        string connectionName,
        string procName,
        CancellationToken ct = default
    );

    /// <summary>
    /// Crée une nouvelle commande SQL.
    /// </summary>
    /// <param name="connectionName">Nom de la connection.</param>
    /// <param name="assembly">Assembly dans lequel chercher la requête SQL.</param>
    /// <param name="resourcePath">Chemin vers le fichier SQL.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Commande SQL.</returns>
    public abstract Task<BaseSqlCommand> GetSqlCommandAsync(
        string connectionName,
        Assembly assembly,
        string resourcePath,
        CancellationToken ct = default
    );

    /// <summary>
    /// Crée une nouvelle commande SQL.
    /// </summary>
    /// <param name="connectionName">Nom de la connection.</param>
    /// <param name="commandName">Nom de la commande.</param>
    /// <param name="commandText">Requête SQL.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Commande SQL.</returns>
    public abstract Task<BaseSqlCommand> GetSqlCommandAsync(
        string connectionName,
        string commandName,
        string commandText,
        CancellationToken ct = default
    );

    /// <summary>
    /// Crée une nouvelle commande SQL.
    /// </summary>
    /// <param name="connectionName">Nom de la connection.</param>
    /// <param name="commandName">Nom de la commande.</param>
    /// <param name="commandType">Type de la commande.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Commande SQL.</returns>
    public abstract Task<BaseSqlCommand> GetSqlCommandAsync(
        string connectionName,
        string commandName,
        CommandType commandType,
        CancellationToken ct = default
    );

    /// <summary>
    /// Récupère la connection pour la datasource demandée.
    /// </summary>
    /// <param name="datasourceName">Nom de la datasource.</param>
    /// <returns>La connection.</returns>
    protected DbConnection GetConnection(string datasourceName)
    {
        var transactionContext = GetTransactionContext();
        transactionContext.Connections.TryGetValue(datasourceName, out var item);
        var (connection, _) = item;

        if (connection == null)
        {
            connection = GetNewConnection(datasourceName);
            var registered = false;
            try
            {
                connection.Open();
                var transaction = connection.BeginTransaction();
                transactionContext.Connections.Add(datasourceName, (connection, transaction));
                registered = true;
            }
            finally
            {
                if (!registered)
                {
                    connection.Dispose();
                }
            }
        }

        return connection;
    }

    /// <summary>
    /// Récupère la connection pour la datasource demandée.
    /// </summary>
    /// <param name="datasourceName">Nom de la datasource.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>La connection.</returns>
    protected async Task<DbConnection> GetConnectionAsync(string datasourceName, CancellationToken ct = default)
    {
        var transactionContext = GetTransactionContext();
        transactionContext.Connections.TryGetValue(datasourceName, out var item);
        var (connection, _) = item;

        if (connection == null)
        {
            connection = GetNewConnection(datasourceName);
            var registered = false;
            try
            {
                await connection.OpenAsync(ct);
                var transaction = await connection.BeginTransactionAsync(ct);
                transactionContext.Connections.Add(datasourceName, (connection, transaction));
                registered = true;
            }
            finally
            {
                if (!registered)
                {
                    await connection.DisposeAsync();
                }
            }
        }

        return connection;
    }

    /// <summary>
    /// Récupère une nouvelle connexion.
    /// </summary>
    /// <param name="datasourceName">Nom de la datasource.</param>
    /// <returns>Connexion.</returns>
    protected abstract DbConnection GetNewConnection(string datasourceName);

    /// <summary>
    /// Récupère la transaction associée à une connexion du scope actif.
    /// </summary>
    /// <param name="connection">Connexion.</param>
    /// <returns>Transaction.</returns>
    protected DbTransaction GetTransaction(DbConnection connection)
    {
        return GetTransactionContext().Connections.Values.Single(item => item.Connection == connection).Transaction;
    }

    private SqlTransactionContext GetTransactionContext()
    {
        return transactionScopeManager.ActiveScope?.GetContext<SqlTransactionContext>()
            ?? throw new InvalidOperationException(
                "Impossible de récupérer une connection en dehors d'un scope de transaction."
            );
    }
}
