using System.Runtime.CompilerServices;
using Kinetix.Monitoring.Core;
using Kinetix.Search.Core;
using Kinetix.Search.Core.Config;
using Kinetix.Search.Core.DocumentModel;
using Kinetix.Search.Core.Querying;
using Kinetix.Search.Elastic.Querying;
using Kinetix.Search.Models;
using Microsoft.Extensions.Logging;
using Nest;

namespace Kinetix.Search.Elastic;

using static AdvancedQueryUtil;

/// <summary>
/// Store ElasticSearch.
/// </summary>
public class ElasticStore(
    DocumentDescriptor documentDescriptor,
    ElasticClient client,
    ElasticManager elasticManager,
    ElasticMappingFactory factory,
    ILogger<ElasticStore> logger,
    FacetHandler facetHandler,
    AnalyticsManager analytics,
    SearchConfig config
) : ISearchStore
{
    /// <inheritdoc cref="ISearchStore.AdvancedCountAsync{TDocument, TCriteria}" />
    public async Task<long> AdvancedCountAsync<TDocument, TCriteria>(
        AdvancedQueryInput<TDocument, TCriteria> input,
        CancellationToken ct = default
    )
        where TDocument : class
        where TCriteria : ICriteria
    {
        ArgumentNullException.ThrowIfNull(input);

        var def = documentDescriptor.GetDefinition(typeof(TDocument));

        /* Requête de filtrage, qui inclus ici le filtre et le post-filtre puisqu'on ne fait pas d'aggrégations. */
        var filterQuery = GetFilterAndPostFilterQuery(def, input);
        return (
            await logger.LogQueryAsync(
                analytics,
                "AdvancedCount",
                (ct) => client.CountAsync<TDocument>(s => s.Query(filterQuery), ct),
                ct
            )
        ).Count;
    }

    /// <inheritdoc cref="ISearchStore.AdvancedQueryAsync{TDocument, TOutput, TCriteria}(AdvancedQueryInput{TDocument, TCriteria}, Func{TDocument, TOutput}, CancellationToken)" />
    public Task<QueryOutput<TOutput>> AdvancedQueryAsync<TDocument, TOutput, TCriteria>(
        AdvancedQueryInput<TDocument, TCriteria> input,
        Func<TDocument, TOutput> documentMapper,
        CancellationToken ct = default
    )
        where TDocument : class
        where TCriteria : ICriteria
    {
        return AdvancedQueryAsync(
            input,
            (d, _) => documentMapper(d),
            filter: null,
            sorts: null,
            sortsAfter: false,
            aggs: null,
            ct: ct
        );
    }

    /// <inheritdoc cref="ISearchStore.AdvancedQueryAsync{TDocument, TOutput, TCriteria}(AdvancedQueryInput{TDocument, TCriteria}, Func{TDocument, IReadOnlyDictionary{string, IReadOnlyCollection{string}}, TOutput}, CancellationToken)" />
    public Task<QueryOutput<TOutput>> AdvancedQueryAsync<TDocument, TOutput, TCriteria>(
        AdvancedQueryInput<TDocument, TCriteria> input,
        Func<TDocument, IReadOnlyDictionary<string, IReadOnlyCollection<string>>, TOutput> documentMapper,
        CancellationToken ct = default
    )
        where TDocument : class
        where TCriteria : ICriteria
    {
        return AdvancedQueryAsync(
            input,
            documentMapper,
            filter: null,
            sorts: null,
            sortsAfter: false,
            aggs: null,
            ct: ct
        );
    }

    /// <inheritdoc cref="ISearchStore.Bulk" />
    public ISearchBulkDescriptor Bulk()
    {
        return new ElasticBulkDescriptor(documentDescriptor, client, logger, analytics);
    }

    /// <inheritdoc cref="ISearchStore.DeleteAsync{TDocument}" />
    public async Task DeleteAsync<TDocument>(object id, bool refresh = true, CancellationToken ct = default)
        where TDocument : class
    {
        await Bulk().Delete<TDocument>(id).RunAsync(refresh, ct);
    }

    /// <inheritdoc cref="ISearchStore.EnsureIndexAsync{TDocument}" />
    public async Task<bool> EnsureIndexAsync<TDocument>(CancellationToken ct = default)
        where TDocument : class
    {
        var def = documentDescriptor.GetDefinition(typeof(TDocument));
        var mapping = new PutMappingDescriptor<TDocument>().Properties(selector =>
            factory.AddFields(selector, def.Fields)
        );

        var indexCreated = await elasticManager.InitIndexAsync<TDocument, DefaultIndexConfigurator>(mapping, ct);

        if (indexCreated)
        {
            await logger.LogQueryAsync(analytics, "Map", (ct) => client.MapAsync<TDocument>(_ => mapping, ct), ct);
        }

        return indexCreated;
    }

    /// <inheritdoc cref="ISearchStore.GetAsync{TDocument}" />
    public async Task<TDocument> GetAsync<TDocument>(object id, CancellationToken ct = default)
        where TDocument : class
    {
        var def = documentDescriptor.GetDefinition(typeof(TDocument));
        return (
            await logger.LogQueryAsync(
                analytics,
                "Get",
                (ct) => client.GetAsync(new DocumentPath<TDocument>(def.PrimaryKey.GetValueFromDocument(id)), ct: ct),
                ct
            )
        ).Source;
    }

    /// <inheritdoc cref="ISearchStore.IndexAsync{TDocument}" />
    public async Task IndexAsync<TDocument>(TDocument document, bool refresh = true, CancellationToken ct = default)
        where TDocument : class
    {
        if (document != null)
        {
            await Bulk().Index(document).RunAsync(refresh, ct);
        }
    }

    /// <inheritdoc cref="ISearchStore.MultiAdvancedQuery" />
    public IMultiAdvancedQueryDescriptor MultiAdvancedQuery()
    {
        return new MultiAdvancedQueryDescriptor(client, documentDescriptor, facetHandler);
    }

    /// <inheritdoc cref="ISearchStore.ResetIndexAsync{TDocument}" />
    public async Task<int> ResetIndexAsync<TDocument>(
        IAsyncEnumerable<TDocument> documents,
        bool partialRebuild,
        ILogger? rebuildLogger = null,
        CancellationToken ct = default
    )
        where TDocument : class
    {
        var indexName = SearchConfig.GetTypeNameForIndex(typeof(TDocument));
        var def = documentDescriptor.GetDefinition(typeof(TDocument));

        /* On vide l'index des documents obsolètes. */
        if (partialRebuild && def.IgnoreOnPartialRebuild?.OlderThanDays > 0 && def.PartialRebuildDate != null)
        {
            rebuildLogger?.LogInformation($"Partial rebuild. Deleting recent documents for {indexName}...");

            var deleteRes = await logger.LogQueryAsync(
                analytics,
                "DeleteAllByQuery",
                (ct) =>
                    client.DeleteByQueryAsync<TDocument>(
                        d =>
                            d.Query(q =>
                                    q.DateRange(d =>
                                        d.Field(def.PartialRebuildDate.FieldName)
                                            .GreaterThan(
                                                DateTime.UtcNow.Date.AddDays(-def.IgnoreOnPartialRebuild.OlderThanDays)
                                            )
                                    )
                                )
                                .Timeout(TimeSpan.FromMinutes(5))
                                .RequestConfiguration(r => r.RequestTimeout(TimeSpan.FromMinutes(5))),
                        ct
                    ),
                ct
            );

            rebuildLogger?.LogInformation($"{deleteRes.Deleted} documents deleted.");
        }

        rebuildLogger?.LogInformation($"Starting indexation for index {indexName}...");

        /* Indexation en cluster */
        var count = 0;

        try
        {
            await elasticManager.OptimizeIndexForReindexAsync<TDocument>(ct);

            await foreach (var cluster in documents.Chunk(config.ClusterSize).WithCancellation(ct))
            {
                await Bulk().IndexMany(cluster).RunAsync(refresh: false, ct);
                count += cluster.Length;
                rebuildLogger?.LogInformation($"{count} documents indexed.");
            }

            rebuildLogger?.LogInformation($"Indexation of index {indexName} is complete.");
        }
        finally
        {
            await elasticManager.RevertOptimizeIndexForReindexAsync<TDocument>(ct);
        }

        return count;
    }

    internal async IAsyncEnumerable<TOutput> AdvancedQueryAll<TDocument, TOutput, TCriteria>(
        AdvancedQueryInput<TDocument, TCriteria> input,
        Func<TDocument, IReadOnlyDictionary<string, IReadOnlyCollection<string>>, TOutput> documentMapper,
        Func<QueryContainerDescriptor<TDocument>, QueryContainer>? filter,
        IEnumerable<Action<SortDescriptor<TDocument>>>? sorts,
        bool sortsAfter,
        [EnumeratorCancellation] CancellationToken ct = default
    )
        where TDocument : class
        where TCriteria : ICriteria
    {
        ArgumentNullException.ThrowIfNull(input);

        var def = documentDescriptor.GetDefinition(typeof(TDocument));

        var pit = await logger.LogQueryAsync(
            analytics,
            "CreatePit",
            (ct) =>
                client.OpenPointInTimeAsync(
                    config.GetIndexNameForType(ElasticConfigBuilder.ServerName, typeof(TDocument)),
                    p => p.KeepAlive("1m"),
                    ct
                ),
            ct
        );

        var pitId = pit.Id;
        try
        {
            object[]? searchAfter = null;

            var search = true;
            do
            {
                var res = await logger.LogQueryAsync(
                    analytics,
                    $"AdvancedQueryWithPit",
                    (ct) =>
                        client.SearchAsync(
                            GetAdvancedQueryDescriptor(
                                def,
                                input,
                                facetHandler,
                                filter,
                                sorts,
                                sortsAfter,
                                pitId: pitId,
                                searchAfter: searchAfter
                            ),
                            ct
                        ),
                    ct
                );

                foreach (var doc in res.Hits)
                {
                    yield return documentMapper(doc.Source, doc.Highlight);
                }

                if (res.Documents.Count == 10000)
                {
                    searchAfter = res.Hits.Last().Sorts.ToArray();
                }
                else
                {
                    search = false;
                }
            } while (search);
        }
        finally
        {
            await logger.LogQueryAsync(
                analytics,
                "DeletePit",
                (ct) => client.ClosePointInTimeAsync(p => p.Id(pitId), ct),
                ct
            );
        }
    }

    internal async Task<QueryOutput<TOutput>> AdvancedQueryAsync<TDocument, TOutput, TCriteria>(
        AdvancedQueryInput<TDocument, TCriteria> input,
        Func<TDocument, IReadOnlyDictionary<string, IReadOnlyCollection<string>>, TOutput> documentMapper,
        Func<QueryContainerDescriptor<TDocument>, QueryContainer>? filter,
        IEnumerable<Action<SortDescriptor<TDocument>>>? sorts,
        bool sortsAfter,
        Action<AggregationContainerDescriptor<TDocument>>? aggs,
        CancellationToken ct = default
    )
        where TDocument : class
        where TCriteria : ICriteria
    {
        ArgumentNullException.ThrowIfNull(input);

        /* Définition du document. */
        var def = documentDescriptor.GetDefinition(typeof(TDocument));

        /* Facettage. */
        var facetDefList = input.FacetQueryDefinition.Facets;
        var hasFacet = facetDefList.Count != 0;
        /* Group */
        var groupFieldName = GetGroupFieldName(input);
        var hasGroup = groupFieldName != null;

        var res = await logger.LogQueryAsync(
            analytics,
            "AdvancedQuery",
            (ct) =>
                client.SearchAsync(
                    GetAdvancedQueryDescriptor(
                        def,
                        input,
                        facetHandler,
                        filter,
                        sorts,
                        sortsAfter,
                        aggs,
                        facetDefList,
                        groupFieldName
                    ),
                    ct
                ),
            ct
        );

        /* Extraction des facettes. */
        var facetListOutput = new List<FacetOutput>();
        if (hasFacet)
        {
            foreach (var facetDef in facetDefList)
            {
                facetListOutput.Add(
                    new FacetOutput
                    {
                        Code = facetDef.Code,
                        Label = facetDef.Label,
                        IsMultiSelectable = facetDef.IsMultiSelectable,
                        IsMultiValued = def.Fields[facetDef.FieldName].IsMultiValued,
                        CanExclude = facetDef.CanExclude,
                        Values = await facetHandler.ExtractFacetItemListAsync(res.Aggregations, facetDef, ct),
                    }
                );
            }
        }

        /* Ajout des valeurs de facettes manquantes (cas d'une valeur demandée par le client non trouvée par la recherche.) */
        foreach (var facet in input.SearchCriteria.SelectMany(sc => sc.Facets ?? new Dictionary<string, FacetInput>()))
        {
            var facetItems = facetListOutput.Single(f => f.Code == facet.Key).Values;
            /* On ajoute un FacetItem par valeur non trouvée, avec un compte de 0. */
            foreach (var value in facet.Value.Selected.Concat(facet.Value.Excluded))
            {
                if (!facetItems.Any(f => f.Code == value))
                {
                    var facetDef = facetDefList.FirstOrDefault(fct => fct.Code == facet.Key);
                    var label =
                        value == FacetConst.NotNullValue ? FacetConst.NotNullLabel
                        : value == FacetConst.NullValue ? FacetConst.NullLabel
                        : facetDef != null ? await facetDef.ResolveLabelAsync(value, ct)
                        : null;

                    facetItems.Add(
                        new FacetItem
                        {
                            Code = value,
                            Label = label!,
                            Count = 0,
                        }
                    );
                }
            }
        }

        /* Extraction des résultats. */
        List<TOutput>? resultList = null;
        var groupResultList = new List<GroupResult<TOutput>>();
        if (hasGroup)
        {
            /* Groupement. */
            var bucket = res.Aggregations.Terms(groupFieldName);
            bucket ??= res.Aggregations.Filter(groupFieldName).Terms(groupFieldName);

            var facetDef = facetDefList.First(f =>
                f.Code == input.SearchCriteria.First(sc => !string.IsNullOrEmpty(sc.Group)).Group
            );

            foreach (var group in bucket.Buckets)
            {
                var list = group
                    .TopHits(TopHitName)
                    .Hits<TDocument>()
                    .Select(d => documentMapper(d.Source, d.Highlight))
                    .ToList();

                groupResultList.Add(
                    new GroupResult<TOutput>
                    {
                        Code = group.Key,
                        Label = await facetDef.ResolveLabelAsync(group.Key, ct),
                        List = list,
                        TotalCount = (int)(group.DocCount ?? 0),
                    }
                );
            }

            // Gestion des modes spéciaux sur les facettes de référence.
            if (
                facetDef is ReferenceFacet<TDocument> rfDef
                && (rfDef.ShowEmptyReferenceValues || rfDef.Ordering == FacetOrdering.ReferenceOrder)
            )
            {
                var referenceValues = (await rfDef.GetReferenceListAsync(ct))
                    .Select(r => new GroupResult<TOutput>
                    {
                        Code = r.Code,
                        Label = r.Label,
                        List = groupResultList.SingleOrDefault(g => g.Code == r.Code)?.List ?? [],
                        TotalCount = groupResultList.SingleOrDefault(g => g.Code == r.Code)?.TotalCount ?? 0,
                    })
                    .ToList();

                if (!rfDef.ShowEmptyReferenceValues)
                {
                    referenceValues = referenceValues.Where(rf => rf.TotalCount > 0).ToList();
                }

                // On est obligé de retrier par derrière.
                groupResultList = facetDef.Ordering switch
                {
                    FacetOrdering.ReferenceOrder => referenceValues,
                    FacetOrdering.CountAscending => referenceValues.OrderBy(fi => fi.TotalCount).ToList(),
                    FacetOrdering.CountDescending => referenceValues.OrderByDescending(fi => fi.TotalCount).ToList(),
                    FacetOrdering.KeyAscending => referenceValues.OrderBy(fi => fi.Code).ToList(),
                    FacetOrdering.KeyDescending => referenceValues.OrderByDescending(fi => fi.Code).ToList(),
                    _ => throw new InvalidOperationException(),
                };
            }

            /* Groupe pour les valeurs missing. */
            var missingBucket = res.Aggregations.Missing(groupFieldName + MissingGroupPrefix);
            missingBucket ??= res.Aggregations.Filter(groupFieldName).Missing(groupFieldName + MissingGroupPrefix);

            var nullDocs = missingBucket
                .TopHits(TopHitName)
                .Hits<TDocument>()
                .Select(d => documentMapper(d.Source, d.Highlight))
                .ToList();
            if (nullDocs.Count != 0)
            {
                groupResultList.Add(
                    new GroupResult<TOutput>
                    {
                        Code = FacetConst.NullValue,
                        Label = input.FacetQueryDefinition.FacetNullValueLabel ?? "focus.search.results.missing",
                        List = nullDocs,
                        TotalCount = (int)missingBucket.DocCount,
                    }
                );
            }
        }
        else
        {
            /* Liste unique. */
            resultList = res.Hits.Select(h => documentMapper(h.Source, h.Highlight)).ToList();
            groupResultList = null;
        }

        /* Construction de la sortie. */
        return new QueryOutput<TOutput>
        {
            List = resultList,
            Facets = facetListOutput,
            Groups = groupResultList,
            SearchFields = def.SearchFields.Select(tf => tf.FieldName).ToList(),
            TotalCount = (int)res.Total,
            Aggregations = res.Aggregations,
        };
    }
}
