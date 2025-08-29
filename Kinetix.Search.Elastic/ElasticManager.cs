using Kinetix.Monitoring.Core;
using Kinetix.Search.Core.Config;
using Kinetix.Search.Core.DocumentModel;
using Microsoft.Extensions.Logging;
using Nest;

namespace Kinetix.Search.Elastic;

/// <summary>
/// Manager pour la gestion d'Elastic Search.
/// </summary>
/// <remarks>
/// Enregistre la configuration d'une connexion base de données.
/// </remarks>
public sealed class ElasticManager(ILogger<ElasticManager> logger, SearchConfig config, ElasticClient client, AnalyticsManager analytics, DocumentDescriptor documentDescriptor)
{
    /// <summary>
    /// Supprime l'index pour le document donné.
    /// </summary>
    public void DeleteIndex<T>()
    {
        logger.LogQuery(analytics, nameof(DeleteIndex), () =>
            client.Indices.Delete(config.GetIndexNameForType(ElasticConfigBuilder.ServerName, typeof(T))));
    }

    /// <summary>
    /// Supprime tous les index.
    /// </summary>
    /// <returns>Ok.</returns>
    public bool DeleteIndexes()
    {
        var response = logger.LogQuery(analytics, nameof(DeleteIndexes), () =>
            client.Indices.Delete($"{config.Servers[ElasticConfigBuilder.ServerName].IndexName}*"));
        return response.Acknowledged;
    }

    /// <summary>
    /// Indique si un index existe.
    /// </summary>
    /// <param name="indexName">Nom de l'index.</param>
    /// <returns><code>True</code> si l'index existe.</returns>
    public bool ExistIndex(string indexName)
    {
        return logger.LogQuery(analytics, nameof(ExistIndex), () => client.Indices.Exists(indexName)).Exists;
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
        var indexName = config.GetIndexNameForType(ElasticConfigBuilder.ServerName, typeof(T));
        var indexExists = ExistIndex(indexName);
        var def = documentDescriptor.GetDefinition(typeof(T));
        var shouldCreate = !indexExists || def.IgnoreOnPartialRebuild == null;

        if (!shouldCreate)
        {
            var properties = typeMapping.Properties;
            var oldProperties = client.Indices.GetMapping<T>().Indices.FirstOrDefault().Value?.Mappings.Properties;

            var mappingExists = oldProperties != null
                && properties.Count == oldProperties.Count
                && oldProperties.Zip(properties, (o, n) =>
                {
                    return o.Key == n.Key && (o.Value, n.Value) switch
                    {
                        (IKeywordProperty okp, IKeywordProperty nkp)
                            => okp.Name == nkp.Name && okp.Index == nkp.Index,
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

            logger.LogQuery(analytics, nameof(InitIndex), () => client.Indices.Create(
                config.GetIndexNameForType(ElasticConfigBuilder.ServerName, typeof(T)),
                new TIndexConfigurator().ConfigureIndex));
        }
        else
        {
            logger.LogInformation($"Creation of {indexName} index skipped : mappings are already up to date.");
        }

        return shouldCreate;
    }

    /// <summary>
    /// Optimise l'index pour une réindexation totale.
    /// </summary>
    public void OptimizeIndexForReindex<T>()
    {
        logger.LogQuery(analytics, nameof(OptimizeIndexForReindex), () => client.Indices.UpdateSettings(
            config.GetIndexNameForType(ElasticConfigBuilder.ServerName, typeof(T)),
            d => d.IndexSettings(i => i.RefreshInterval(30_000).NumberOfReplicas(0))));
    }

    /// <summary>
    /// Ping un node ES.
    /// </summary>
    public void PingNode()
    {
        logger.LogQuery(analytics, nameof(PingNode), () => client.Ping());
    }

    /// <summary>
    /// Rétabli les paramètres par défaut de l'index après une réindexation totale.
    /// </summary>
    public void RevertOptimizeIndexForReindex<T>()
    {
        logger.LogQuery(analytics, nameof(RevertOptimizeIndexForReindex), () => client.Indices.UpdateSettings(
            config.GetIndexNameForType(ElasticConfigBuilder.ServerName, typeof(T)),
            d => d.IndexSettings(i => i.RefreshInterval(1_000).NumberOfReplicas(1))));
    }
}
