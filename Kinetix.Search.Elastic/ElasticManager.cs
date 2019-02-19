using System;
using System.Collections.Generic;
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
        private readonly ElasticClient _client;
        private readonly SearchConfig _config;
        private readonly ICollection<Type> _documentTypes;
        private readonly ILogger<ElasticManager> _logger;

        /// <summary>
        /// Enregistre la configuration d'une connexion base de données.
        /// </summary>
        /// <param name="searchSettings">Configuration.</param>
        public ElasticManager(ILogger<ElasticManager> logger, IOptions<SearchConfig> searchConfig, ElasticClient client, ICollection<Type> documentTypes)
        {
            _client = client;
            _logger = logger;
            _config = searchConfig.Value;
            _documentTypes = documentTypes;
        }

        /// <summary>
        /// Initialise un index pour le document donné avec la configuration Analyser/Tokenizer.
        /// </summary>
        public void InitIndex<T, TIndexConfigurator>()
            where TIndexConfigurator : IIndexConfigurator, new()
        {
            if (ExistIndex(_client, _config.GetIndexNameForType(ElasticConfigBuilder.ServerName, typeof(T))))
            {
                DeleteIndex<T>();
            }

            _logger.LogQuery("CreateIndex", () =>
                _client.CreateIndex(new TIndexConfigurator().CreateIndexRequest(_config.GetIndexNameForType(ElasticConfigBuilder.ServerName, typeof(T)))));
        }

        /// <summary>
        /// Supprime l'index pour le document donné.
        /// </summary>
        public void DeleteIndex<T>()
        {
            _logger.LogQuery("DeleteIndex", () =>
                _client.DeleteIndex(_config.GetIndexNameForType(ElasticConfigBuilder.ServerName, typeof(T))));
        }

        /// <summary>
        /// Indique si un index existe.
        /// </summary>
        /// <param name="client">Client ES pour la datasource voulue.</param>
        /// <param name="indexName">Nom de l'index.</param>
        /// <returns><code>True</code> si l'index existe.</returns>
        public bool ExistIndex(ElasticClient client, string indexName)
        {
            return _logger.LogQuery("IndexExists", () => client.IndexExists(indexName)).Exists;
        }

        /// <summary>
        /// Ping un node ES.
        /// </summary>
        /// <param name="dataSourceName">Nom de la datasource.</param>
        public void PingNode(string dataSourceName)
        {
            _logger.LogQuery("Ping", () => _client.Ping());
        }
    }
}
