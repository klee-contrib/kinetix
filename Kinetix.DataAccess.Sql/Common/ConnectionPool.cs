using System.Data;
using System.Reflection;
using Kinetix.Services;

namespace Kinetix.DataAccess.Sql.Common;

/// <summary>
/// Pool de connections SQL de base.
/// </summary>
/// <remarks>
/// Constructeur.
/// </remarks>
/// <param name="transactionScopeManager">Composant injecté.</param>
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
    /// Récupère la connection pour la datasource demandée.
    /// </summary>
    /// <param name="datasourceName">Nom de la datasource.</param>
    /// <returns>La connection.</returns>
    protected IDbConnection GetConnection(string datasourceName)
    {
        var transactionContext = transactionScopeManager.ActiveScope?.GetContext<SqlTransactionContext>();

        if (transactionContext == null)
        {
            throw new InvalidOperationException("Impossible de récupérer une connection en dehors d'un scope de transaction.");
        }

        transactionContext.Connections.TryGetValue(datasourceName, out var connection);

        if (connection == null)
        {
            connection = GetNewConnection(datasourceName);
            transactionContext.Connections.Add(datasourceName, connection);
            connection.Open();
        }

        if (connection.State == ConnectionState.Closed)
        {
            connection.Open();
        }

        return connection;
    }

    /// <summary>
    /// Récupère une nouvelle connexion.
    /// </summary>
    /// <param name="datasourceName">Nom de la datasource.</param>
    /// <returns>Connexion.</returns>
    protected abstract IDbConnection GetNewConnection(string datasourceName);
}
