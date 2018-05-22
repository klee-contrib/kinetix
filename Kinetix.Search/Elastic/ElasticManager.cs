using System;
using System.Collections.Generic;
using System.Reflection;
using Elasticsearch.Net;
using Kinetix.Search.ComponentModel;
using Kinetix.Search.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using Nest.JsonNetSerializer;
using Newtonsoft.Json;

namespace Kinetix.Search.Elastic
{
    /// <summary>
    /// Manager pour la gestion d'Elastic Search.
    /// </summary>
    public sealed class ElasticManager
    {
        private readonly ILogger<ElasticManager> _logger;
        private readonly SearchConfig _searchConfig;
        private readonly ICollection<Type> _documentTypes;
        private readonly ICollection<JsonConverter> _jsonConverters;

        /// <summary>
        /// Enregistre la configuration d'une connexion base de données.
        /// </summary>
        /// <param name="searchSettings">Configuration.</param>
        public ElasticManager(ILogger<ElasticManager> logger, IOptions<SearchConfig> searchConfig, ICollection<Type> documentTypes, ICollection<JsonConverter> jsonConverters)
        {
            _logger = logger;
            _searchConfig = searchConfig.Value;
            _documentTypes = documentTypes;
            _jsonConverters = jsonConverters;
        }

        public string GetIndexNameForType(string dataSourceName, Type documentType)
        {
            var connSettings = LoadSearchSettings(dataSourceName);
            var attribute = documentType.GetCustomAttribute<SearchDocumentTypeAttribute>();
            return $"{connSettings.IndexName}_{attribute.DocumentTypeName}";
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
            var settings = new ConnectionSettings(
                new SingleNodeConnectionPool(node),
                (b, s) => new JsonNetSerializer(b, s, () =>
                {
                    var js = new JsonSerializerSettings();
                    if (_jsonConverters != null)
                    {
                        foreach (var converter in _jsonConverters)
                        {
                            js.Converters.Add(converter);
                        }
                    }
                    return js;
                }))
                .DefaultIndex(connSettings.IndexName)
                .DisableDirectStreaming();

            foreach (var documentType in _documentTypes)
            {
                settings.DefaultMappingFor(documentType, m => m.IndexName(GetIndexNameForType(dataSourceName, documentType)));
            }

            return new ElasticClient(settings);
        }

        /// <summary>
        /// Initialise un index avec la configuration Analyser/Tokenizer.
        /// </summary>
        /// <param name="dataSourceName">Nom de la datasource.</param>
        /// <param name="configurator">Configurateur.</param>
        public void InitIndexes(string dataSourceName, IIndexConfigurator configurator)
        {
            var settings = LoadSearchSettings(dataSourceName);
            var client = ObtainClient(dataSourceName);
            foreach (var docType in _documentTypes)
            {
                var indexName = GetIndexNameForType(dataSourceName, docType);
                DeleteIndex(client, indexName);
                var res = client.CreateIndex(GetIndexNameForType(dataSourceName, docType), configurator.Configure);
                res.CheckStatus(_logger, "CreateIndex");
            }
        }

        /// <summary>
        /// Supprime un index.
        /// </summary>
        /// <param name="client">Client ES pour la datasource voulue.</param>
        /// <param name="indexName">Nom de l'index.</param>
        public void DeleteIndex(ElasticClient client, string indexName)
        {
            if (ExistIndex(client, indexName))
            {
                var res = client.DeleteIndex(indexName);
                if (res.ApiCall.HttpStatusCode == 404)
                {
                    throw new ElasticException($"The {indexName} index to delete doesn't exist.");
                }

                res.CheckStatus(_logger, "DeleteIndex");
            }
        }

        /// <summary>
        /// Indique si un index existe.
        /// </summary>
        /// <param name="client">Client ES pour la datasource voulue.</param>
        /// <param name="indexName">Nom de l'index.</param>
        /// <returns><code>True</code> si l'index existe.</returns>
        public bool ExistIndex(ElasticClient client, string indexName)
        {
            var res = client.IndexExists(indexName);
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
