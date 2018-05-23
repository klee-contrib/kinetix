using System;
using Microsoft.Extensions.DependencyInjection;

namespace Kinetix.Search
{
    /// <summary>
    /// Manager pour les brokers de recherche.
    /// </summary>
    public sealed class SearchManager
    {
        private readonly string _defaultDataSourceName;
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
        public ISearchStore<T> GetStore<T>(string dataSourceName = null)
            where T : class, new()
        {
            return _provider.GetService<ISearchStore<T>>()
                .RegisterDataSource(dataSourceName ?? _defaultDataSourceName);
        }
    }
}
