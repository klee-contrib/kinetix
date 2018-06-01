using System;
using System.Collections.Generic;
using Kinetix.Monitoring.Abstractions;
using Kinetix.Search.Contract;
using Microsoft.Extensions.DependencyInjection;

namespace Kinetix.Search
{
    /// <summary>
    /// Manager pour les brokers de recherche.
    /// </summary>
    public sealed class SearchManager
    {
        private readonly string _defaultDataSourceName;
        private readonly Dictionary<string, ISearchBroker> _brokerMap = new Dictionary<string, ISearchBroker>();
        private readonly IServiceProvider _provider;

        /// <summary>
        /// Constructeur.
        /// </summary>
        public SearchManager(string defaultDataSourceName, IServiceProvider provider)
        {
            if (string.IsNullOrEmpty(defaultDataSourceName))
            {
                throw new ArgumentNullException(nameof(defaultDataSourceName));
            }

            _defaultDataSourceName = defaultDataSourceName;
            _provider = provider;
        }

        /// <summary>
        /// Retourne l'instance du broker associé au type.
        /// </summary>
        /// <typeparam name="T">Type du broker.</typeparam>
        /// <param name="dataSourceName">Source de données : source par défaut si nulle.</param>
        /// <returns>Le broker.</returns>
        public ISearchBroker<T> GetBroker<T>(string dataSourceName = null)
            where T : class, new()
        {
            var dsName = dataSourceName ?? _defaultDataSourceName;

            var key = typeof(T).AssemblyQualifiedName + "/" + dsName;

            if (_brokerMap.TryGetValue(key, out var broker))
            {
                return (ISearchBroker<T>)broker;
            }

            lock (_brokerMap)
            {
                if (_brokerMap.TryGetValue(key, out broker))
                {
                    return (ISearchBroker<T>)broker;
                }

                var searchBroker = new SearchBroker<T>(_provider.GetService<ISearchStore<T>>().RegisterDataSource(dsName), _provider.GetService<IAnalyticsManager>());
                _brokerMap[key] = searchBroker;
                return searchBroker;
            }
        }
    }
}
