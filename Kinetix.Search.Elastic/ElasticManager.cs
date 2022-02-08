using Kinetix.Monitoring.Core;
using Kinetix.Search.Core.Config;
using Microsoft.Extensions.Logging;
using Nest;

namespace Kinetix.Search.Elastic;

/// <summary>
/// Manager pour la gestion d'Elastic Search.
/// </summary>
public sealed class ElasticManager
{
    private readonly AnalyticsManager _analytics;
    private readonly ElasticClient _client;
    private readonly SearchConfig _config;
    private readonly ILogger<ElasticManager> _logger;

    /// <summary>
    /// Enregistre la configuration d'une connexion base de données.
    /// </summary>
    public ElasticManager(ILogger<ElasticManager> logger, SearchConfig config, ElasticClient client, AnalyticsManager analytics)
    {
        _analytics = analytics;
        _client = client;
        _config = config;
        _logger = logger;
    }

    /// <summary>
    /// Initialise un index pour le document donné avec la configuration Analyser/Tokenizer.
    /// </summary>
    /// <param name="typeMapping">Mapping à comparer avec l'existant, pour ne pas recréer si identique.</param>
    /// <returns>True si l'index a bien été (re)créé.</returns>
    public bool InitIndex<T, TIndexConfigurator>(ITypeMapping typeMapping)
        where T : class
        where TIndexConfigurator : IIndexConfigurator, new()
    {
        var indexName = _config.GetIndexNameForType(ElasticConfigBuilder.ServerName, typeof(T));
        var indexExists = ExistIndex(_client, indexName);
        var shouldCreate = !indexExists;

        if (indexExists)
        {
            var properties = typeMapping.Properties;
            var oldProperties = _client.Indices.GetMapping<T>().Indices.FirstOrDefault().Value?.Mappings.Properties;

            var mappingExists = oldProperties != null
                && properties.Count == oldProperties.Count
                && oldProperties.Zip(properties, (o, n) =>
                {
                    return o.Key == n.Key && (o.Value, n.Value) switch
                    {
                        (IKeywordProperty okp, IKeywordProperty nkp)
                            => okp.Name == nkp.Name && okp.Index == okp.Index,
                        (ITextProperty otp, ITextProperty ntp)
                            => otp.Name == ntp.Name && otp.Analyzer == ntp.Analyzer && otp.SearchAnalyzer == ntp.SearchAnalyzer,
                        (INumberProperty onp, INumberProperty nnp)
                            => onp.Name == nnp.Name && onp.Type == nnp.Type && onp.Index == nnp.Index,
                        (IDateProperty odp, IDateProperty ndp)
                            => odp.Name == ndp.Name && odp.Index == ndp.Index && odp.Format == ndp.Format,
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

            _logger.LogQuery(_analytics, "CreateIndex", () => _client.Indices.Create(
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
        _logger.LogQuery(_analytics, "DeleteIndex", () =>
            _client.Indices.Delete(_config.GetIndexNameForType(ElasticConfigBuilder.ServerName, typeof(T))));
    }

    /// <summary>
    /// Supprime tous les index.
    /// </summary>
    /// <returns>Ok.</returns>
    public bool DeleteIndexes()
    {
        var response = _logger.LogQuery(_analytics, "DeleteIndexes", () =>
            _client.Indices.Delete($"{_config.Servers[ElasticConfigBuilder.ServerName].IndexName}*"));
        return response.Acknowledged;
    }

    /// <summary>
    /// Indique si un index existe.
    /// </summary>
    /// <param name="client">Client ES pour la datasource voulue.</param>
    /// <param name="indexName">Nom de l'index.</param>
    /// <returns><code>True</code> si l'index existe.</returns>
    public bool ExistIndex(ElasticClient client, string indexName)
    {
        return _logger.LogQuery(_analytics, "IndexExists", () => client.Indices.Exists(indexName)).Exists;
    }

    /// <summary>
    /// Ping un node ES.
    /// </summary>
    public void PingNode()
    {
        _logger.LogQuery(_analytics, "Ping", () => _client.Ping());
    }
}
