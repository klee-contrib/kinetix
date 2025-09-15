using Kinetix.DataAccess.Sql.Broker;
using Kinetix.DataAccess.Sql.Common;
using Kinetix.Services;

namespace Kinetix.DataAccess.Sql.Common.Broker;

/// <summary>
/// Manager pour les brokers.
/// </summary>
/// <remarks>
/// Constructeur.
/// </remarks>
/// <param name="connectionPool">Composant injecté.</param>
/// <param name="transactionScopeManager">Composant injecté.</param>
public abstract class BrokerManager(ConnectionPool connectionPool, TransactionScopeManager transactionScopeManager)
{
    private readonly Dictionary<string, IBroker> _brokerMap = [];

    /// <summary>
    /// Pool de connection.
    /// </summary>
    public ConnectionPool ConnectionPool { get; } = connectionPool;

    /// <summary>
    /// Retourne l'instance du broker associé au type.
    /// </summary>
    /// <typeparam name="T">Type du broker.</typeparam>
    /// <param name="dataSourceName">Source de données : source par défaut si nulle.</param>
    /// <returns>Le broker.</returns>
    public IBroker<T> GetBroker<T>(string dataSourceName = SqlConfig.DefaultConnection)
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
                var broker = new StandardBroker<T>(transactionScopeManager, GetStore<T>(dataSourceName));
                _brokerMap.Add(key, broker);
                return broker;
            }
        }
    }

    /// <summary>
    /// Récupère un store SQL pour un broker.
    /// </summary>
    /// <typeparam name="T">Type de bean.</typeparam>
    /// <param name="dataSourceName">Nom de la datasource.</param>
    /// <returns>Le store.</returns>
    protected abstract IStore<T> GetStore<T>(string dataSourceName)
        where T : class, new();
}
