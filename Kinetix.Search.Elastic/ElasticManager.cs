using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Mapping;
using Kinetix.Monitoring.Core;
using Kinetix.Search.Core.Config;
using Kinetix.Search.Core.DocumentModel;
using Microsoft.Extensions.Logging;

namespace Kinetix.Search.Elastic;

/// <summary>
/// Manager pour la gestion d'Elastic Search.
/// </summary>
public sealed class ElasticManager
{
    private readonly AnalyticsManager _analytics;
    private readonly ElasticsearchClient _client;
    private readonly SearchConfig _config;
    private readonly DocumentDescriptor _documentDescriptor;
    private readonly ILogger<ElasticManager> _logger;

    /// <summary>
    /// Enregistre la configuration d'une connexion base de données.
    /// </summary>
    public ElasticManager(ILogger<ElasticManager> logger, SearchConfig config, ElasticsearchClient client, AnalyticsManager analytics, DocumentDescriptor documentDescriptor)
    {
        _analytics = analytics;
        _client = client;
        _config = config;
        _documentDescriptor = documentDescriptor;
        _logger = logger;
    }

    /// <summary>
    /// Initialise un index pour le document donné avec la configuration Analyser/Tokenizer.
    /// </summary>
    /// <param name="typeMapping">Mapping à comparer avec l'existant, pour ne pas recréer si identique.</param>
    /// <returns>True si l'index a bien été (re)créé.</returns>
    public bool InitIndex<T, TIndexConfigurator>(TypeMapping typeMapping)
        where T : class
        where TIndexConfigurator : IIndexConfigurator, new()
    {
        var indexName = _config.GetIndexNameForType(ElasticConfigBuilder.ServerName, typeof(T));
        var indexExists = ExistIndex(indexName);
        var def = _documentDescriptor.GetDefinition(typeof(T));
        var shouldCreate = !indexExists || def.IgnoreOnPartialRebuild == null;

        if (!shouldCreate)
        {
            var properties = typeMapping.Properties;
            var oldProperties = _client.Indices.GetMapping<T>(x => x.AllowNoIndices(false)).Indices.FirstOrDefault().Value?.Mappings.Properties;

            var mappingExists = oldProperties != null
                && properties.Count() == oldProperties.Count()
                && oldProperties.Zip(properties, (o, n) =>
                {
                    return o.Key == n.Key && (o.Value, n.Value) switch
                    {
                        (KeywordProperty okp, KeywordProperty nkp)
                            => okp.Index == okp.Index,
                        (TextProperty otp, TextProperty ntp)
                            => otp.Analyzer == ntp.Analyzer && otp.SearchAnalyzer == ntp.SearchAnalyzer,
                        (IntegerNumberProperty onp, IntegerNumberProperty nnp)
                            => onp.Type == nnp.Type && onp.Index == nnp.Index,
                        (FloatNumberProperty onp, FloatNumberProperty nnp)
                             => onp.Type == nnp.Type && onp.Index == nnp.Index,
                        (DateProperty odp, DateProperty ndp)
                            => odp.Index == ndp.Index && odp.Format == ndp.Format,
                        _ => false
                    };
                }).All(res => res);

            shouldCreate = !mappingExists;
        }

        if (shouldCreate)
        {
            if (indexExists)
            {
                DeleteIndex<T>();
            }

            _logger.LogQuery(_analytics, nameof(InitIndex), () => _client.Indices.Create(
                _config.GetIndexNameForType(ElasticConfigBuilder.ServerName, typeof(T)),
                new TIndexConfigurator().ConfigureIndex));
        }
        else
        {
            _logger.LogInformation($"Creation of {indexName} index skipped : mappings are already up to date.");
        }

        return shouldCreate;
    }

    /// <summary>
    /// Supprime l'index pour le document donné.
    /// </summary>
    public void DeleteIndex<T>()
    {
        _logger.LogQuery(_analytics, nameof(DeleteIndex), () =>
            _client.Indices.Delete(_config.GetIndexNameForType(ElasticConfigBuilder.ServerName, typeof(T))));
    }

    /// <summary>
    /// Supprime tous les index.
    /// </summary>
    /// <returns>Ok.</returns>
    public bool DeleteIndexes()
    {
        var response = _logger.LogQuery(_analytics, nameof(DeleteIndexes), () =>
            _client.Indices.Delete($"{_config.Servers[ElasticConfigBuilder.ServerName].IndexName}*"));
        return response.Acknowledged;
    }

    /// <summary>
    /// Indique si un index existe.
    /// </summary>
    /// <param name="indexName">Nom de l'index.</param>
    /// <returns><code>True</code> si l'index existe.</returns>
    public bool ExistIndex(string indexName)
    {
        return _logger.LogQuery(_analytics, nameof(ExistIndex), () => _client.Indices.Exists(indexName)).Exists;
    }

    /// <summary>
    /// Optimise l'index pour une réindexation totale.
    /// </summary>
    public void OptimizeIndexForReindex<T>()
    {
        _logger.LogQuery(_analytics, nameof(OptimizeIndexForReindex), () => _client.Indices.UpdateSettings(
            _config.GetIndexNameForType(ElasticConfigBuilder.ServerName, typeof(T)),
            d => d.IndexSettings(i => i.RefreshInterval(30_000).NumberOfReplicas(0))));
    }

    /// <summary>
    /// Ping un node ES.
    /// </summary>
    public void PingNode()
    {
        _logger.LogQuery(_analytics, nameof(PingNode), () => _client.Ping());
    }

    /// <summary>
    /// Rétabli les paramètres par défaut de l'index après une réindexation totale.
    /// </summary>
    public void RevertOptimizeIndexForReindex<T>()
    {
        _logger.LogQuery(_analytics, nameof(RevertOptimizeIndexForReindex), () => _client.Indices.UpdateSettings(
            _config.GetIndexNameForType(ElasticConfigBuilder.ServerName, typeof(T)),
            d => d.IndexSettings(i => i.RefreshInterval(1_000).NumberOfReplicas(1))));
    }
}
