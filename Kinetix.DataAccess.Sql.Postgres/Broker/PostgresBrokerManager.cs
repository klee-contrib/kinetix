using Kinetix.DataAccess.Sql.Broker;
using Kinetix.Services;
using Microsoft.Extensions.Logging;

namespace Kinetix.DataAccess.Sql.Postgres.Broker;

/// <summary>
/// Manager pour les brokers.
/// </summary>
internal class PostgresBrokerManager : BrokerManager
{
    private readonly ILogger<BrokerManager> _logger;

    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="connectionPool">Composant injecté.</param>
    /// <param name="transactionScopeManager">Composant injecté.</param>
    /// <param name="logger">Composant injecté.</param>
    public PostgresBrokerManager(ConnectionPool connectionPool, TransactionScopeManager transactionScopeManager, ILogger<BrokerManager> logger)
        : base(connectionPool, transactionScopeManager)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    protected override IStore<T> GetStore<T>(string dataSourceName)
    {
        return new PostgresStore<T>(dataSourceName, ConnectionPool, _logger);
    }
}
