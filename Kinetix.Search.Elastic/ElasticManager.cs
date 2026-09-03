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
/// <remarks>
/// Enregistre la configuration d'une connexion base de données.
/// </remarks>
public sealed class ElasticManager(
    ILogger<ElasticManager> logger,
    SearchConfig config,
    ElasticsearchClient client,
    AnalyticsManager analytics,
    DocumentDescriptor documentDescriptor
)
{
    /// <summary>
    /// Supprime l'index pour le document donné.
    /// </summary>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Task.</returns>
    public async Task DeleteIndexAsync<T>(CancellationToken ct = default)
    {
        await logger.LogQueryAsync(
            analytics,
            nameof(DeleteIndexAsync),
            (ct) =>
                client.Indices.DeleteAsync(config.GetIndexNameForType(ElasticConfigBuilder.ServerName, typeof(T)), ct),
            ct
        );
    }

    /// <summary>
    /// Supprime tous les index.
    /// </summary>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Ok.</returns>
    public async Task<bool> DeleteIndexesAsync(CancellationToken ct = default)
    {
        var response = await logger.LogQueryAsync(
            analytics,
            nameof(DeleteIndexesAsync),
            (ct) => client.Indices.DeleteAsync($"{config.Servers[ElasticConfigBuilder.ServerName].IndexName}*", ct),
            ct
        );
        return response.Acknowledged;
    }

    /// <summary>
    /// Indique si un index existe.
    /// </summary>
    /// <param name="indexName">Nom de l'index.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns><code>True</code> si l'index existe.</returns>
    public async Task<bool> ExistIndexAsync(string indexName, CancellationToken ct = default)
    {
        return (
            await logger.LogQueryAsync(
                analytics,
                nameof(ExistIndexAsync),
                (ct) => client.Indices.ExistsAsync(indexName, ct),
                ct
            )
        ).Exists;
    }

    /// <summary>
    /// Initialise un index pour le document donné avec la configuration Analyser/Tokenizer.
    /// </summary>
    /// <param name="properties">Mapping à comparer avec l'existant, pour ne pas recréer si identique.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>True si l'index a bien été (re)créé.</returns>
    public async Task<bool> InitIndexAsync<T, TIndexConfigurator>(Properties properties, CancellationToken ct = default)
        where T : class
        where TIndexConfigurator : IIndexConfigurator, new()
    {
        var indexName = config.GetIndexNameForType(ElasticConfigBuilder.ServerName, typeof(T));
        var indexExists = await ExistIndexAsync(indexName, ct);
        var def = documentDescriptor.GetDefinition(typeof(T));
        var shouldCreate = !indexExists || def.IgnoreOnPartialRebuild == null;

        if (!shouldCreate)
        {
            var oldProperties = (await client.Indices.GetMappingAsync<T>(ct)).Mappings;

            var mappingExists =
                oldProperties != null
                && properties.Count() == oldProperties.Count
                && oldProperties
                    .Zip(
                        properties,
                        (o, n) =>
                        {
                            return false;

                            // TODO : Comparaison.
                            ////return o.Key == n.Key
                            ////    && (o.Value, n.Value) switch
                            ////    {
                            ////        (DynamicMapping okp, KeywordProperty nkp) => okp.Index == nkp.Index,
                            ////        (ITextProperty otp, TextProperty ntp) => otp.Analyzer == ntp.Analyzer
                            ////            && otp.SearchAnalyzer == ntp.SearchAnalyzer,
                            ////        (INumberProperty onp, FloatNumberProperty nnp) => onp.Type == nnp.Type
                            ////            && onp.Index == nnp.Index,
                            ////        (INumberProperty onp, IntegerNumberProperty nnp) => onp.Type == nnp.Type
                            ////            && onp.Index == nnp.Index,
                            ////        (IDateProperty odp, DateProperty ndp) => odp.Index == ndp.Index
                            ////            && odp.Format == ndp.Format,
                            ////        _ => false,
                            ////    };
                        }
                    )
                    .All(res => res);

            shouldCreate = !mappingExists;
        }

        if (shouldCreate)
        {
            if (indexExists)
            {
                await DeleteIndexAsync<T>(ct);
            }

            await logger.LogQueryAsync(
                analytics,
                nameof(InitIndexAsync),
                (ct) =>
                    client.Indices.CreateAsync(
                        config.GetIndexNameForType(ElasticConfigBuilder.ServerName, typeof(T)),
                        new TIndexConfigurator().ConfigureIndex,
                        ct
                    ),
                ct
            );
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
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Task.</returns>
    public async Task OptimizeIndexForReindexAsync<T>(CancellationToken ct = default)
    {
        await logger.LogQueryAsync(
            analytics,
            nameof(OptimizeIndexForReindexAsync),
            (ct) =>
                client.Indices.PutSettingsAsync(
                    config.GetIndexNameForType(ElasticConfigBuilder.ServerName, typeof(T)),
                    d => d.Settings(i => i.RefreshInterval(30_000).NumberOfReplicas(0)),
                    ct
                ),
            ct
        );
    }

    /// <summary>
    /// Ping un node ES.
    /// </summary>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Task.</returns>
    public async Task PingNodeAsync(CancellationToken ct = default)
    {
        await logger.LogQueryAsync(analytics, nameof(PingNodeAsync), (ct) => client.PingAsync(ct), ct);
    }

    /// <summary>
    /// Rétabli les paramètres par défaut de l'index après une réindexation totale.
    /// </summary>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Task.</returns>
    public async Task RevertOptimizeIndexForReindexAsync<T>(CancellationToken ct = default)
    {
        await logger.LogQueryAsync(
            analytics,
            nameof(RevertOptimizeIndexForReindexAsync),
            (ct) =>
                client.Indices.PutSettingsAsync(
                    config.GetIndexNameForType(ElasticConfigBuilder.ServerName, typeof(T)),
                    d => d.Settings(i => i.RefreshInterval(1_000).NumberOfReplicas(1)),
                    ct
                ),
            ct
        );
    }
}
