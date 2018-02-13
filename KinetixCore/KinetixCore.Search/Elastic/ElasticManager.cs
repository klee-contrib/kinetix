using System;
using Kinetix.Search.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;

namespace Kinetix.Search.Elastic
{
    /// <summary>
    /// Manager pour la gestion d'Elastic Search.
    /// </summary>
    public sealed class ElasticManager
    {
        private readonly ILogger<ElasticManager> _logger;
        private readonly SearchConfig _searchConfig;

        /// <summary>
        /// Enregistre la configuration d'une connexion base de données.
        /// </summary>
        /// <param name="searchSettings">Configuration.</param>
        public ElasticManager(ILogger<ElasticManager> logger, IOptions<SearchConfig> searchConfig)
        {
            _logger = logger;
            _searchConfig = searchConfig.Value;
        }

        /// <summary>
        /// Obtient un client ElasticSearch pour une datasource donnée.
        /// </summary>
        /// <param name="dataSourceName">Nom de la datasource.</param>
        /// <returns>Client Elastic.</returns>
        public ElasticClient ObtainClient(string dataSourceName)
        {
            var connSettings = LoadSearchSettings(dataSourceName);
            var node = new Uri(connSettings.NodeUri);
            var settings = new ConnectionSettings(node)
                .DefaultIndex(connSettings.IndexName)
                .DisableDirectStreaming();
            return new ElasticClient(settings);
        }

        /// <summary>
        /// Initialise un index avec la configuration Analyser/Tokenizer.
        /// </summary>
        /// <param name="dataSourceName">Nom de la datasource.</param>
        /// <param name="configurator">Configurateur.</param>
        public void InitIndex(string dataSourceName, IIndexConfigurator configurator)
        {
            var settings = LoadSearchSettings(dataSourceName);
            var client = ObtainClient(dataSourceName);
            var res = client.CreateIndex(settings.IndexName, configurator.Configure);
            res.CheckStatus(_logger, "CreateIndex");
        }

        /// <summary>
        /// Supprime un index.
        /// </summary>
        /// <param name="dataSourceName">Nom de la datasource.</param>
        public void DeleteIndex(string dataSourceName)
        {
            var settings = LoadSearchSettings(dataSourceName);
            var client = ObtainClient(dataSourceName);
            var res = client.DeleteIndex(settings.IndexName);
            if (res.ApiCall.HttpStatusCode == 404)
            {
                throw new ElasticException("The " + settings.IndexName + " index to delete doesn't exist.");
            }

            res.CheckStatus(_logger, "DeleteIndex");
        }

        /// <summary>
        /// Indique si un index existe.
        /// </summary>
        /// <param name="dataSourceName">Nom de la datasource.</param>
        /// <returns><code>True</code> si l'index existe.</returns>
        public bool ExistIndex(string dataSourceName)
        {
            var settings = LoadSearchSettings(dataSourceName);
            var client = ObtainClient(dataSourceName);
            var res = client.IndexExists(settings.IndexName);
            res.CheckStatus(_logger, "IndexExists");
            return res.Exists;
        }

        /// <summary>
        /// Ping un node ES.
        /// </summary>
        /// <param name="dataSourceName">Nom de la datasource.</param>
        public void PingNode(string dataSourceName)
        {
            var settings = LoadSearchSettings(dataSourceName);
            var client = ObtainClient(dataSourceName);
            var res = client.Ping();
            res.CheckStatus(_logger, "Ping");
        }

        /// <summary>
        /// Charge les paramètres de connexion.
        /// </summary>
        /// <param name="dataSourceName">Nom de la DataSource.</param>
        /// <returns>Paramètres de connexion.</returns>
        internal SearchConfigItem LoadSearchSettings(string dataSourceName)
        {
            if (_searchConfig.Servers.TryGetValue(dataSourceName, out var server))
            {
                return server;
            }
            else
            {
                throw new ArgumentException($"{dataSourceName} introuvable dans la configuration.");
            }
        }
    }
}
