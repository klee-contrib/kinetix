using System;
using System.Collections.Generic;
using System.Linq;
using Kinetix.Search.ComponentModel;
using Kinetix.Search.Elastic.Faceting;
using Kinetix.Search.Elastic.Querying;
using Kinetix.Search.MetaModel;
using Kinetix.Search.Model;
using Microsoft.Extensions.Logging;
using Nest;

namespace Kinetix.Search.Elastic
{
    using static AdvancedQueryUtil;

    /// <summary>
    /// Store ElasticSearch.
    /// </summary>
    public class ElasticStore : ISearchStore
    {
        /// <summary>
        /// Taille de cluster pour l'insertion en masse.
        /// </summary>
        private const int ClusterSize = 1000;

        private readonly ElasticClient _client;
        private readonly DocumentDescriptor _documentDescriptor;
        private readonly ILogger<ElasticStore> _logger;
        private readonly ElasticMappingFactory _factory;
        private readonly IFacetHandler _portfolioHandler;
        private readonly IFacetHandler _standardHandler;

        public ElasticStore(ILogger<ElasticStore> logger, DocumentDescriptor documentDescriptor, ElasticClient client, ElasticMappingFactory factory, StandardFacetHandler standardHandler, PortfolioFacetHandler portfolioHandler)
        {
            _client = client;
            _documentDescriptor = documentDescriptor;
            _factory = factory;
            _logger = logger;
            _portfolioHandler = portfolioHandler;
            _standardHandler = standardHandler;
        }

        /// <inheritdoc cref="ISearchStore.CreateDocumentType" />
        public void CreateDocumentType<TDocument>()
            where TDocument : class
        {
            var def = _documentDescriptor.GetDefinition(typeof(TDocument));

            _logger.LogInformation("Create Document type : " + def.DocumentTypeName);
            _logger.LogQuery("Map", () =>
                _client.Map<TDocument>(x => x
                     .Type(def.DocumentTypeName)
                     .Properties(selector => _factory.AddFields(selector, def.Fields))));
        }

        /// <inheritdoc cref="ISearchStore.Get{TDocument}(string)" />
        public TDocument Get<TDocument>(string id)
            where TDocument : class
        {
            var def = _documentDescriptor.GetDefinition(typeof(TDocument));
            return _logger.LogQuery("Get", () => _client.Get(new DocumentPath<TDocument>(id).Type(def.DocumentTypeName))).Source;
        }

        /// <inheritdoc cref="ISearchStore.Get{TDocument}(TDocument)" />
        public TDocument Get<TDocument>(TDocument bean)
            where TDocument : class
        {
            var def = _documentDescriptor.GetDefinition(typeof(TDocument));
            return _logger.LogQuery("Get", () => _client.Get(new DocumentPath<TDocument>(def.PrimaryKey.GetValue(bean).ToString()).Type(def.DocumentTypeName))).Source;
        }

        /// <inheritdoc cref="ISearchStore.Bulk" />
        public ISearchBulkDescriptor Bulk()
        {
            return new ElasticBulkDescriptor(_documentDescriptor, _client, _logger);
        }

        /// <inheritdoc cref="ISearchStore.Delete(string)" />
        public void Delete(string id)
        {
            Bulk().Delete(id).Run(true);
        }

        /// <inheritdoc cref="ISearchStore.Delete{TDocument}(TDocument)" />
        public void Delete<TDocument>(TDocument bean)
            where TDocument : class
        {
            Bulk().Delete(bean).Run(true);
        }

        /// <inheritdoc cref="ISearchStore.Index" />
        public void Index<TDocument>(TDocument document)
            where TDocument : class
        {
            Bulk().Index(document).Run(true);
        }

        /// <inheritdoc cref="ISearchStore.IndexAll" />
        public void IndexAll<TDocument>(IEnumerable<TDocument> documentList)
            where TDocument : class
        {
            /* On vide l'index. */
            _logger.LogQuery("DeleteAll", () => _client.DeleteByQuery<TDocument>(d => d
                .Type(_documentDescriptor.GetDefinition(typeof(TDocument)).DocumentTypeName)));

            if (!documentList.Any())
            {
                return;
            }

            /* Découpage en cluster. */
            var total = documentList.Count();
            var left = total % ClusterSize;
            var clusterNb = (total - left) / ClusterSize + (left > 0 ? 1 : 0);

            for (var i = 1; i <= clusterNb; i++)
            {
                /* Extraction du cluster. */
                var cluster = documentList
                    .Skip((i - 1) * ClusterSize)
                    .Take(ClusterSize);

                /* Indexation en masse du cluster. */
                Bulk().IndexMany(cluster).Run();
            }
        }

        /// <inheritdoc cref="ISearchStore.AdvancedQuery{TDocument, TOutput, TCriteria}(AdvancedQueryInput{TDocument, TCriteria}, Func{TDocument, TOutput})" />
        public QueryOutput<TOutput> AdvancedQuery<TDocument, TOutput, TCriteria>(AdvancedQueryInput<TDocument, TCriteria> input, Func<TDocument, TOutput> documentMapper)
            where TDocument : class
            where TCriteria : Criteria, new()
        {
            return AdvancedQuery(input, documentMapper, new Func<QueryContainerDescriptor<TDocument>, QueryContainer>[0]);
        }

        /// <summary>
        /// Effectue une recherche avancée.
        /// </summary>
        /// <param name="input">Entrée de la recherche.</param>
        /// <param name="documentMapper">Mapper pour convertir le document dans le bon type de sortie.</param>
        /// <param name="filters">Filtres NEST additionnels.</param>
        /// <returns>Sortie de la recherche.</returns>
        public QueryOutput<TOutput> AdvancedQuery<TDocument, TOutput, TCriteria>(AdvancedQueryInput<TDocument, TCriteria> input, Func<TDocument, TOutput> documentMapper, params Func<QueryContainerDescriptor<TDocument>, QueryContainer>[] filters)
            where TDocument : class
            where TCriteria : Criteria, new()
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            /* Définition du document. */
            var def = _documentDescriptor.GetDefinition(typeof(TDocument));

            /* Facettage. */
            var facetDefList = GetFacetDefinitionList(input, GetHandler);
            var hasFacet = facetDefList.Any();

            /* Group */
            var groupFieldName = GetGroupFieldName(def, input);
            var hasGroup = !string.IsNullOrEmpty(input.ApiInput.Group);

            var res = _logger.LogQuery("AdvancedQuery", () => _client.Search(
                GetAdvancedQueryDescriptor(def, input, GetHandler, facetDefList, groupFieldName, filters)));

            /* Extraction des facettes. */
            var facetListOutput = new List<FacetOutput>();
            if (hasFacet)
            {
                var aggs = res.Aggregations;
                foreach (var facetDef in facetDefList)
                {
                    facetListOutput.Add(new FacetOutput
                    {
                        Code = facetDef.Code,
                        Label = facetDef.Label,
                        IsMultiSelectable = facetDef.IsMultiSelectable,
                        Values = GetHandler(facetDef).ExtractFacetItemList(aggs, facetDef, res.Total)
                    });
                }
            }

            /* Ajout des valeurs de facettes manquantes (cas d'une valeur demandée par le client non trouvée par la recherche.) */
            if (input.ApiInput.Facets != null)
            {
                foreach (var facet in input.ApiInput.Facets)
                {
                    var facetItems = facetListOutput.Single(f => f.Code == facet.Key).Values;
                    /* On ajoute un FacetItem par valeur non trouvée, avec un compte de 0. */
                    foreach (var value in facet.Value)
                    {
                        if (!facetItems.Any(f => f.Code == value))
                        {
                            facetItems.Add(new FacetItem
                            {
                                Code = value,
                                Label = facetDefList.FirstOrDefault(fct => fct.Code == facet.Key)?.ResolveLabel(value),
                                Count = 0
                            });
                        }
                    }
                }
            }

            /* Extraction des résultats. */
            var resultList = new List<TOutput>();
            var groupResultList = new List<GroupResult<TOutput>>();
            if (hasGroup)
            {
                /* Groupement. */
                var bucket = res.Aggregations.Terms(groupFieldName);
                if (bucket == null)
                {
                    bucket = res.Aggregations.Filter(groupFieldName).Terms(groupFieldName);
                }

                foreach (var group in bucket.Buckets)
                {
                    var list = group.TopHits(TopHitName).Documents<TDocument>().Select(documentMapper).ToList();
                    groupResultList.Add(new GroupResult<TOutput>
                    {
                        Code = group.Key.ToString(),
                        Label = facetDefList.First(f => f.Code == input.ApiInput.Group).ResolveLabel(group.Key),
                        List = list,
                        TotalCount = (int)group.DocCount
                    });
                }

                /* Groupe pour les valeurs missing. */
                var missingBucket = res.Aggregations.Missing(groupFieldName + MissingGroupPrefix);
                if (missingBucket == null)
                {
                    missingBucket = res.Aggregations.Filter(groupFieldName + MissingGroupPrefix).Missing(groupFieldName + MissingGroupPrefix);
                }

                var nullDocs = missingBucket.TopHits(TopHitName).Documents<TDocument>().Select(documentMapper).ToList();
                if (nullDocs.Any())
                {
                    groupResultList.Add(new GroupResult<TOutput>
                    {
                        Code = FacetConst.NullValue,
                        Label = input.FacetQueryDefinition.FacetNullValueLabel ?? "focus.search.results.missing",
                        List = nullDocs,
                        TotalCount = (int)missingBucket.DocCount
                    });
                }

                resultList = null;
            }
            else
            {
                /* Liste unique. */
                resultList = res.Documents.Select(documentMapper).ToList();
                groupResultList = null;
            }

            /* Construction de la sortie. */
            var output = new QueryOutput<TOutput>
            {
                List = resultList,
                Facets = facetListOutput,
                Groups = groupResultList,
                TotalCount = (int)res.Total
            };

            return output;
        }

        /// <inheritdoc cref="ISearchStore.MultiAdvancedQuery" />
        public IMultiAdvancedQueryDescriptor MultiAdvancedQuery()
        {
            return new MultiAdvancedQueryDescriptor(_client, _documentDescriptor, GetHandler);
        }

        /// <inheritdoc cref="ISearchStore.AdvancedCount" />
        public long AdvancedCount<TDocument, TCriteria>(AdvancedQueryInput<TDocument, TCriteria> input)
            where TDocument : class
            where TCriteria : Criteria, new()
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var def = _documentDescriptor.GetDefinition(typeof(TDocument));

            /* Requête de filtrage, qui inclus ici le filtre et le post-filtre puisqu'on ne fait pas d'aggrégations. */
            var filterQuery = GetFilterAndPostFilterQuery(def, input, GetHandler);
            return _logger.LogQuery("AdvancedCount", () => _client
                .Count<TDocument>(s => s

                    /* Index / type document. */
                    .Type(def.DocumentTypeName)

                    /* Critère de filtrage. */
                    .Query(filterQuery)))
                .Count;
        }

        /// <summary>
        /// Recherche directement via le client ElasticSearch.
        /// </summary>
        /// <param name="descriptor">Search descriptor.</param>
        /// <returns>Search response.</returns>
        public ISearchResponse<TDocument> Search<TDocument>(Func<SearchDescriptor<TDocument>, ISearchRequest> descriptor)
            where TDocument : class
        {
            var def = _documentDescriptor.GetDefinition(typeof(TDocument));
            return _logger.LogQuery("Search", () => _client.Search((SearchDescriptor<TDocument> s) => descriptor(s.Type(def.DocumentTypeName))));
        }

        /// <summary>
        /// Renvoie le handler de facet pour une définition de facet.
        /// </summary>
        /// <param name="def">Définition de facet.</param>
        /// <returns>Handler.</returns>
        private IFacetHandler GetHandler(IFacetDefinition def)
        {
            return def.GetType() == typeof(PortfolioFacet) ? _portfolioHandler : _standardHandler;
        }
    }
}
