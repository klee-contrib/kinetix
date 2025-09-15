using Kinetix.DataAccess.Sql.Broker;
using Kinetix.DataAccess.Sql.Common;
using Kinetix.DataAccess.Sql.Common.Broker;
using Kinetix.Services;
using Microsoft.Extensions.Logging;

namespace Kinetix.DataAccess.Sql.Postgres.Broker;

/// <summary>
/// Manager pour les brokers.
/// </summary>
/// <remarks>
/// Constructeur.
/// </remarks>
/// <param name="connectionPool">Composant injecté.</param>
/// <param name="transactionScopeManager">Composant injecté.</param>
/// <param name="logger">Composant injecté.</param>
internal class PostgresBrokerManager(
    ConnectionPool connectionPool,
    TransactionScopeManager transactionScopeManager,
    ILogger<BrokerManager> logger
) : BrokerManager(connectionPool, transactionScopeManager)
{
    /// <inheritdoc />
    protected override IStore<T> GetStore<T>(string dataSourceName)
    {
        return new PostgresStore<T>(dataSourceName, ConnectionPool, logger);
    }
}
