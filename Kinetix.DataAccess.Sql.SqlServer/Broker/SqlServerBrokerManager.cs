using Kinetix.DataAccess.Sql.Broker;
using Kinetix.Services;
using Microsoft.Extensions.Logging;

namespace Kinetix.DataAccess.Sql.SqlServer.Broker
{
    /// <summary>
    /// Manager pour les brokers.
    /// </summary>
    public class SqlServerBrokerManager : BrokerManager
    {
        private readonly ILogger<BrokerManager> _logger;

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="connectionPool">Composant injecté.</param>
        /// <param name="transactionScopeManager">Composant injecté.</param>
        /// <param name="logger">Composant injecté.</param>
        public SqlServerBrokerManager(ConnectionPool connectionPool, TransactionScopeManager transactionScopeManager, ILogger<BrokerManager> logger)
            : base(connectionPool, transactionScopeManager)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        protected override IStore<T> GetStore<T>(string dataSourceName)
        {
            return new SqlServerStore<T>(dataSourceName, ConnectionPool, _logger);
        }
    }
}
