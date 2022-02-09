using System.Collections.Generic;
using Kinetix.Data.SqlClient;
using Kinetix.Services;
using Microsoft.Extensions.Logging;

namespace Kinetix.Broker
{
    /// <summary>
    /// Manager pour les brokers.
    /// </summary>
    public sealed class BrokerManager
    {
        private readonly Dictionary<string, IBroker> _brokerMap = new Dictionary<string, IBroker>();
        private readonly ILogger<BrokerManager> _logger;
        private readonly ServiceScopeManager _serviceScopeManager;

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="dataSources">Sources de données (la première sera la source par défaut).</param>
        public BrokerManager(ConnectionPool connectionPool, ServiceScopeManager serviceScopeManager, ILogger<BrokerManager> logger)
        {
            ConnectionPool = connectionPool;
            _logger = logger;
            _serviceScopeManager = serviceScopeManager;
        }

        public ConnectionPool ConnectionPool { get; }

        /// <summary>
        /// Retourne l'instance du broker associé au type.
        /// </summary>
        /// <typeparam name="T">Type du broker.</typeparam>
        /// <param name="dataSourceName">Source de données : source par défaut si nulle.</param>
        /// <returns>Le broker.</returns>
        public IBroker<T> GetBroker<T>(string dataSourceName = SqlServerConfig.DefaultConnection)
            where T : class, new()
        {
            var key = typeof(T).AssemblyQualifiedName + "/" + dataSourceName;
            if (_brokerMap.TryGetValue(key, out var basicBroker))
            {
                return (IBroker<T>)basicBroker;
            }

            lock (_brokerMap)
            {
                if (_brokerMap.TryGetValue(key, out basicBroker))
                {
                    return (IBroker<T>)basicBroker;
                }
                else
                {
                    var broker = new StandardBroker<T>(_serviceScopeManager, new SqlServerStore<T>(dataSourceName, ConnectionPool, _logger));
                    _brokerMap.Add(key, broker);
                    return broker;
                }
            }
        }
    }
}
