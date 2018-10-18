using System;
using System.Collections.Generic;
using System.Linq;
using Kinetix.Search.ComponentModel;
using Kinetix.Search.Elastic.Faceting;
using Kinetix.Search.MetaModel;
using Kinetix.Search.Model;
using Microsoft.Extensions.Logging;
using Nest;

namespace Kinetix.Search.Elastic
{
    using static ElasticQueryBuilder;

    /// <summary>
    /// Store ElasticSearch.
    /// </summary>
    /// <typeparam name="TDocument">Type du document.</typeparam>
    public class ElasticStore<TDocument> : ISearchStore<TDocument>
        where TDocument : class
    {
        /// <summary>
        /// Taille de cluster pour l'insertion en masse.
        /// </summary>
        private const int ClusterSize = 1000;

        private const string MissingGroupPrefix = "_Missing";
        private const string GroupAggs = "groupAggs";

        /// <summary>
        /// Nom de l'aggrégation des top hits pour le groupement.
        /// </summary>
        private const string _topHitName = "top";

        private static ILogger<ElasticStore<TDocument>> _logger;

        /// <summary>
        /// Usine à mapping ElasticSearch.
        /// </summary>
        private readonly ElasticMappingFactory _factory;

        /// <summary>
        /// Handler des facettes standard.
        /// </summary>
        private readonly IFacetHandler<TDocument> _standardHandler;

        /// <summary>
        /// Handler des facettes portefeuille.
        /// </summary>
        private readonly IFacetHandler<TDocument> _portfolioHandler;

        /// <summary>
        /// Nom de la source de données.
        /// </summary>
        private string _dataSourceName;

        /// <summary>
        /// Définition du document.
        /// </summary>
        private readonly DocumentDefinition _definition;

        private readonly ElasticClient _client;

        /// <summary>
        /// Nom du type du document.
        /// </summary>
        private readonly string _documentTypeName;

        public ElasticStore(ILogger<ElasticStore<TDocument>> logger, DocumentDescriptor documentDescriptor, ElasticClient client, ElasticMappingFactory factory)
        {
            _definition = documentDescriptor.GetDefinition(typeof(TDocument));
            _documentTypeName = _definition.DocumentTypeName;
            _client = client;
            _factory = factory;
            _logger = logger;
            _standardHandler = new StandardFacetHandler<TDocument>(_definition);
            _portfolioHandler = new PortfolioFacetHandler<TDocument>(_definition);
        }

        /// <inheritdoc cref="ISearchStore{TDocument}.CreateDocumentType" />
        public void CreateDocumentType()
        {
            _logger.LogInformation("Create Document type : " + _documentTypeName);
            _logger.LogQuery("Map", () =>
                _client.Map<TDocument>(x => x
                     .Type(_documentTypeName)
                     .Properties(selector => _factory.AddFields(selector, _definition.Fields))));
        }

        /// <inheritdoc cref="ISearchStore{TDocument}.Get(string)" />
        public TDocument Get(string id)
        {
            return _logger.LogQuery("Get", () => _client.Get(CreateDocumentPath(id))).Source;
        }

        /// <inheritdoc cref="ISearchStore{TDocument}.Get(TDocument)" />
        public TDocument Get(TDocument bean)
        {
            return _logger.LogQuery("Get", () => _client.Get(CreateDocumentPath(bean))).Source;
        }

        /// <inheritdoc cref="ISearchStore{TDocument}.Put" />
        public void Put(TDocument document)
        {
            _logger.LogQuery("Index", () =>
                _client.Index(FormatSortFields(document), x => x
                    .Type(_documentTypeName)
                    .Id(_definition.PrimaryKey.GetValue(document).ToString())));
        }

        /// <inheritdoc cref="ISearchStore{TDocument}.PutAll" />
        public void PutAll(IEnumerable<TDocument> documentList)
        {
            if (documentList == null)
            {
                throw new ArgumentNullException(nameof(documentList));
            }

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
                _logger.LogQuery("Bulk", () => _client.Bulk(x =>
                {
                    foreach (var document in cluster)
                    {
                        var id = _definition.PrimaryKey.GetValue(document).ToString();
                        x.Index<TDocument>(y => y
                            .Document(FormatSortFields(document))
                            .Type(_documentTypeName)
                            .Id(id));
                    }
                    return x;
                }));
            }
        }

        /// <inheritdoc cref="ISearchStore{TDocument}.Remove(string)" />
        public void Remove(string id)
        {
            _logger.LogQuery("Delete", () => _client.Delete(CreateDocumentPath(id)));
        }

        /// <inheritdoc cref="ISearchStore{TDocument}.Remove(TDocument)" />
        public void Remove(TDocument bean)
        {
            _logger.LogQuery("Delete", () => _client.Delete(CreateDocumentPath(bean)));
        }

        /// <inheritdoc cref="ISearchStore{TDocument}.Flush" />
        public void Flush()
        {
            /* SEY : Non testé. */
            _logger.LogQuery("DeleteAll", () => _client.DeleteByQuery<TDocument>(x => x.Type(_documentTypeName)));
        }

        /// <inheritdoc cref="ISearchStore{TDocument}.AdvancedQuery{TOutput, TCriteria}(AdvancedQueryInput{TDocument, TCriteria}, Func{TDocument, TOutput})" />
        public QueryOutput<TOutput> AdvancedQuery<TOutput, TCriteria>(AdvancedQueryInput<TDocument, TCriteria> input, Func<TDocument, TOutput> documentMapper)
            where TCriteria : Criteria
        {
            return AdvancedQuery(input, documentMapper, new Func<QueryContainerDescriptor<TDocument>, QueryContainer>[0]);
        }

        public QueryOutput<TOutput> AdvancedQuery<TOutput, TCriteria>(AdvancedQueryInput<TDocument, TCriteria> input, Func<TDocument, TOutput> documentMapper, params Func<QueryContainerDescriptor<TDocument>, QueryContainer>[] filters)
            where TCriteria : Criteria
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var apiInput = input.ApiInput;

            /* Tri */
            var sortDef = GetSortDefinition(input);

            /* Requêtes de filtrage. */
            var filterQuery = GetFilterQuery(input, filters);
            var postFilterQuery = GetPostFilterSubQuery(input);

            /* Facettage. */
            var facetDefList = GetFacetDefinitionList(input);
            var hasFacet = facetDefList.Any();
            var portfolio = input.Portfolio;

            /* Group */
            var groupFieldName = GetGroupFieldName(input);
            var hasGroup = !string.IsNullOrEmpty(apiInput.Group);

            /* Pagination. */
            var skip = apiInput.Skip ?? 0;
            var size = hasGroup ? 0 : apiInput.Top ?? 1000; // TODO Paramétrable ?

            var res = _logger.LogQuery("AdvancedQuery", () => _client
                .Search<TDocument>(s =>
                {
                    s
                        /* Index / type document. */
                        .Type(_documentTypeName)

                        /* Pagination */
                        .From(skip)
                        .Size(size)

                        /* Critère de filtrage. */
                        .Query(filterQuery)

                        /* Critère de post-filtrage. */
                        .PostFilter(postFilterQuery);

                    /* Tri */
                    if (sortDef.HasSort)
                    {
                        s.Sort(x => x.Field(sortDef.FieldName, sortDef.Order));
                    }

                    /* Aggrégations. */
                    if (hasFacet || hasGroup)
                    {
                        s.Aggregations(a =>
                        {
                            if (hasFacet)
                            {
                                /* Facettage. */
                                foreach (var facetDef in facetDefList)
                                {
                                    GetHandler(facetDef).DefineAggregation(a, facetDef, facetDefList, input.ApiInput.Facets, portfolio);
                                }
                            }
                            if (hasGroup)
                            {
                                /* Groupement. */
                                a.Filter(GroupAggs, f => f
                                    /* Critère de post-filtrage répété sur les groupes, puisque ce sont des agrégations qui par définition ne sont pas affectées par le post-filtrage. */
                                    .Filter(postFilterQuery)
                                    .Aggregations(aa => aa
                                        /* Groupement. */
                                        .Terms(groupFieldName, st => st
                                            .Field(groupFieldName)
                                            .Aggregations(g => g.TopHits(_topHitName, x => x.Size(input.GroupSize))))
                                        /* Groupement pour les valeurs nulles */
                                        .Missing(groupFieldName + MissingGroupPrefix, st => st
                                            .Field(groupFieldName)
                                            .Aggregations(g => g.TopHits(_topHitName, x => x.Size(input.GroupSize))))));
                            }
                            return a;
                        });
                    }

                    return s;
                }));

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
                var bucket = (BucketAggregate)res.Aggregations.Filter(GroupAggs)[groupFieldName];
                foreach (KeyedBucket<object> group in bucket.Items)
                {
                    var list = ((TopHitsAggregate)group[_topHitName]).Documents<TDocument>().Select(documentMapper).ToList();
                    groupResultList.Add(new GroupResult<TOutput>
                    {
                        Code = group.Key.ToString(),
                        Label = facetDefList.First(f => f.Code == apiInput.Group).ResolveLabel(group.Key),
                        List = list,
                        TotalCount = (int)group.DocCount
                    });
                }

                /* Groupe pour les valeurs null. */
                var nullBucket = (SingleBucketAggregate)res.Aggregations.Filter(GroupAggs)[groupFieldName + MissingGroupPrefix];
                var nullTopHitAgg = (TopHitsAggregate)nullBucket[_topHitName];
                var nullDocs = nullTopHitAgg.Documents<TDocument>().Select(documentMapper).ToList();
                if (nullDocs.Any())
                {
                    groupResultList.Add(new GroupResult<TOutput>
                    {
                        Code = FacetConst.NullValue,
                        Label = input.FacetQueryDefinition.FacetNullValueLabel ?? "focus.search.results.missing",
                        List = nullDocs,
                        TotalCount = (int)nullBucket.DocCount
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

        /// <inheritdoc cref="ISearchStore{TDocument}.AdvancedCount" />
        public long AdvancedCount<TCriteria>(AdvancedQueryInput<TDocument, TCriteria> input)
            where TCriteria : Criteria
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            /* Requête de filtrage, qui inclus ici le filtre et le post-filtre puisqu'on ne fait pas d'aggrégations. */
            var filterQuery = BuildAndQuery(GetFilterQuery(input), GetPostFilterSubQuery(input));
            return _logger.LogQuery("AdvancedCount", () => _client
                .Count<TDocument>(s => s

                    /* Index / type document. */
                    .Type(_documentTypeName)

                    /* Critère de filtrage. */
                    .Query(filterQuery)))
                .Count;
        }

        /// <inheritdoc cref="ISearchStore{TDocument}.Search" />
        public ISearchResponse<TDocument> Search(Func<SearchDescriptor<TDocument>, ISearchRequest> selector)
        {
            return _logger.LogQuery("Search", () => _client.Search((SearchDescriptor<TDocument> s) => selector(s.Type(_documentTypeName))));
        }

        /// <summary>
        /// Créé la requête de filtrage.
        /// </summary>
        /// <param name="input">Entrée.</param>
        /// <returns>Requête de filtrage.</returns>
        private Func<QueryContainerDescriptor<TDocument>, QueryContainer> GetFilterQuery<TCriteria>(AdvancedQueryInput<TDocument, TCriteria> input, params Func<QueryContainerDescriptor<TDocument>, QueryContainer>[] filters)
            where TCriteria : Criteria
        {
            var textSubQuery = GetTextSubQuery(input);
            var securitySubQuery = GetSecuritySubQuery(input);
            var filterSubQuery = GetFilterSubQuery(input);
            var monoValuedFacetsSubQuery = GetFacetSelectionSubQuery(input);
            return BuildAndQuery(
                new[] { textSubQuery, securitySubQuery, filterSubQuery, monoValuedFacetsSubQuery }
                .Concat(filters).ToArray());
        }

        /// <summary>
        /// Crée la sous requête pour les champs de filtre.
        /// </summary>
        /// <param name="input">Entrée.</param>
        /// <returns>Sous-requête.</returns>
        private Func<QueryContainerDescriptor<TDocument>, QueryContainer> GetFilterSubQuery<TCriteria>(AdvancedQueryInput<TDocument, TCriteria> input)
            where TCriteria : Criteria
        {
            var beanProperties = typeof(TDocument).GetProperties();
            var criteriaProperties = typeof(TCriteria).GetProperties();

            var filterList = new List<Func<QueryContainerDescriptor<TDocument>, QueryContainer>>();

            foreach (var entry in beanProperties)
            {
                var propName = entry.Name;
                var propValue = input.AdditionalCriteria != null
                    ? entry.GetValue(input.AdditionalCriteria)?.ToString()
                    : null;

                if (string.IsNullOrWhiteSpace(propValue))
                {
                    propValue = criteriaProperties.SingleOrDefault(p => p.Name == propName)?.GetValue(input.ApiInput.Criteria)?.ToString();
                }

                if (!string.IsNullOrWhiteSpace(propValue))
                {
                    if (propValue == "True")
                    {
                        propValue = "true";
                    }

                    var field = _definition.Fields[propName];

                    switch (field.Indexing)
                    {
                        case SearchFieldIndexing.FullText:
                            filterList.Add(BuildFullTextSearch<TDocument>(propValue, field.FieldName));
                            break;
                        case SearchFieldIndexing.Term:
                        case SearchFieldIndexing.Terms:
                            filterList.Add(BuildFilter<TDocument>(field.FieldName, propValue));
                            break;
                        default:
                            throw new ElasticException($"Cannot filter on fields that are not indexed as FullText, Term or Terms. Field: {field.FieldName}");
                    }
                }
            }

            return BuildAndQuery(filterList.ToArray());
        }

        /// <summary>
        /// Créé la sous-requête pour le champ textuel.
        /// </summary>
        /// <param name="input">Entrée.</param>
        /// <returns>Sous-requête.</returns>
        private Func<QueryContainerDescriptor<TDocument>, QueryContainer> GetTextSubQuery<TCriteria>(AdvancedQueryInput<TDocument, TCriteria> input)
            where TCriteria : Criteria
        {
            var criteria = input.ApiInput.Criteria;
            var value = criteria?.Query;

            /* Absence de texte ou joker : sous-requête vide. */
            if (string.IsNullOrEmpty(value) || value == "*")
            {
                return q => q;
            }

            /* Vérifie la présence d'au moins un champ textuel. */
            if (!_definition.TextFields.Any())
            {
                throw new ElasticException("The Document \"" + _definition.DocumentTypeName + "\" needs at lease one Search category field to allow Query.");
            }

            /* Constuit la sous requête. */
            return BuildFullTextSearch<TDocument>(value, _definition.TextFields.Select(f => f.FieldName).ToArray());
        }

        /// <summary>
        /// Créé la sous-requête le filtrage de sécurité.
        /// </summary>
        /// <param name="input">Entrée.</param>
        /// <returns>Sous-requête.</returns>
        private Func<QueryContainerDescriptor<TDocument>, QueryContainer> GetSecuritySubQuery<TCriteria>(AdvancedQueryInput<TDocument, TCriteria> input)
            where TCriteria : Criteria
        {
            var value = input.Security;

            /* Absence de filtrage de sécurité : sous-requêt vide. */
            if (string.IsNullOrEmpty(value))
            {
                return q => q;
            }

            /* Vérifie la présence d'un champ de sécurité. */
            var fieldDesc = _definition.SecurityField;
            if (fieldDesc == null)
            {
                throw new ElasticException("The Document \"" + _definition.DocumentTypeName + "\" needs a Security category field to allow Query with security filtering.");
            }

            /* Constuit la sous requête. */
            return BuildInclusiveInclude<TDocument>(fieldDesc.FieldName, value);
        }

        /// <summary>
        /// Créé la sous-requête le filtrage par sélection de facette non multi-sélectionnables.
        /// </summary>
        /// <param name="input">Entrée.</param>
        /// <returns>Sous-requête.</returns>
        private Func<QueryContainerDescriptor<TDocument>, QueryContainer> GetFacetSelectionSubQuery<TCriteria>(AdvancedQueryInput<TDocument, TCriteria> input)
            where TCriteria : Criteria
        {
            var facetList = input.ApiInput.Facets;
            if (facetList == null || !facetList.Any())
            {
                return q => q;
            }

            /* Créé une sous-requête par facette. */
            var facetSubQueryList = facetList
                .Select(f =>
                {
                    /* Récupère la définition de la facette non multi-sélectionnable. */
                    var def = input.FacetQueryDefinition.Facets.SingleOrDefault(x => x.IsMultiSelectable == false && x.Code == f.Key);
                    if (def == null)
                    {
                        return null;
                    }

                    /* La facette n'est pas multi-sélectionnable donc on prend direct la première valeur. */
                    var s = f.Value[0];
                    return GetHandler(def).CreateFacetSubQuery(s, def, input.Portfolio);
                })
                .Where(f => f != null)
                .ToArray();

            if (facetSubQueryList.Any())
            {
                /* Concatène en "ET" toutes les sous-requêtes. */
                return BuildAndQuery(facetSubQueryList);
            }
            else
            {
                return q => q;
            }
        }

        /// <summary>
        /// Créé la sous-requête de post-filtrage pour les facettes multi-sélectionnables.
        /// </summary>
        /// <param name="input">Entrée.</param>
        /// <returns>Sous-requête.</returns>
        private Func<QueryContainerDescriptor<TDocument>, QueryContainer> GetPostFilterSubQuery<TCriteria>(AdvancedQueryInput<TDocument, TCriteria> input)
            where TCriteria : Criteria
        {
            var facetList = input.ApiInput.Facets;
            if (facetList == null || !facetList.Any())
            {
                return q => q;
            }

            /* Créé une sous-requête par facette */
            var facetSubQueryList = facetList
                .Select(f =>
                {
                    /* Récupère la définition de la facette multi-sélectionnable. */
                    var def = input.FacetQueryDefinition.Facets.SingleOrDefault(x => x.IsMultiSelectable == true && x.Code == f.Key);
                    if (def == null)
                    {
                        return null;
                    }

                    var handler = GetHandler(def);
                    /* On fait un "OR" sur toutes les valeurs sélectionnées. */
                    return BuildOrQuery(f.Value.Select(s => handler.CreateFacetSubQuery(s, def, input.Portfolio)).ToArray());
                })
                .Where(f => f != null)
                .ToArray();

            if (facetSubQueryList.Any())
            {
                /* Concatène en "ET" toutes les sous-requêtes. */
                return BuildAndQuery(facetSubQueryList);
            }
            else
            {
                return q => q;
            }
        }

        /// <summary>
        /// Obtient la liste des facettes.
        /// </summary>
        /// <param name="input">Entrée.</param>
        /// <returns>Définitions de facettes.</returns>
        private ICollection<IFacetDefinition> GetFacetDefinitionList<TCriteria>(AdvancedQueryInput<TDocument, TCriteria> input)
            where TCriteria : Criteria
        {
            var groupFacetName = input.ApiInput.Group;
            var list = input.FacetQueryDefinition != null ? input.FacetQueryDefinition.Facets : new List<IFacetDefinition>();

            /* Recherche de la facette de groupement. */
            string groupFieldName = null;
            if (!string.IsNullOrEmpty(groupFacetName))
            {
                var groupFacetDef = input.FacetQueryDefinition.Facets.SingleOrDefault(x => x.Code == groupFacetName);
                if (groupFacetDef == null)
                {
                    throw new ElasticException("No facet \"" + groupFacetName + "\" to group on.");
                }

                groupFieldName = groupFacetDef.FieldName;
            }

            foreach (var facetDef in list)
            {
                /* Vérifie que le champ à facetter existe sur le document. */
                GetHandler(facetDef).CheckFacet(facetDef);
            }

            return list;
        }

        /// <summary>
        /// Obtient la définition du tri.
        /// </summary>
        /// <param name="input">Entrée.</param>
        /// <returns>Définition du tri.</returns>
        private SortDefinition GetSortDefinition<TCriteria>(AdvancedQueryInput<TDocument, TCriteria> input)
            where TCriteria : Criteria
        {
            var fieldName = input.ApiInput.SortFieldName;

            /* Cas de l'absence de tri. */
            if (string.IsNullOrEmpty(fieldName))
            {
                return new SortDefinition();
            }

            /* Vérifie la présence du champ. */
            if (!_definition.Fields.HasProperty(fieldName))
            {
                throw new ElasticException("The Document \"" + _definition.DocumentTypeName + "\" is missing a \"" + fieldName + "\" property to sort on.");
            }

            return new SortDefinition
            {
                FieldName = _definition.Fields[fieldName].FieldName,
                Order = input.ApiInput.SortDesc ? SortOrder.Descending : SortOrder.Ascending
            };
        }

        /// <summary>
        /// Obtient le nom du champ pour le groupement.
        /// </summary>
        /// <param name="input">Entrée.</param>
        /// <returns>Nom du champ.</returns>
        private string GetGroupFieldName<TCriteria>(AdvancedQueryInput<TDocument, TCriteria> input)
            where TCriteria : Criteria
        {
            var groupFacetName = input.ApiInput.Group;

            /* Pas de groupement. */
            if (string.IsNullOrEmpty(groupFacetName))
            {
                return null;
            }

            /* Recherche de la facette de groupement. */
            var facetDef = input.FacetQueryDefinition.Facets.SingleOrDefault(x => x.Code == groupFacetName);
            if (facetDef == null)
            {
                throw new ElasticException("No facet " + groupFacetName + " to group on.");
            }

            var fieldName = facetDef.FieldName;

            /* Vérifie la présence du champ. */
            if (!_definition.Fields.HasProperty(fieldName))
            {
                throw new ElasticException("The Document \"" + _definition.DocumentTypeName + "\" is missing a \"" + fieldName + "\" property to group on.");
            }

            return _definition.Fields[fieldName].FieldName;
        }

        /// <summary>
        /// Créé un DocumentPath.
        /// </summary>
        /// <param name="id">ID du document.</param>
        /// <returns>Le DocumentPath.</returns>
        private DocumentPath<TDocument> CreateDocumentPath(string id)
        {
            return new DocumentPath<TDocument>(id).Type(_documentTypeName);
        }

        /// <summary>
        /// Créé un DocumentPath.
        /// </summary>
        /// <param name="id">ID du document.</param>
        /// <returns>Le DocumentPath.</returns>
        private DocumentPath<TDocument> CreateDocumentPath(TDocument document)
        {
            return new DocumentPath<TDocument>(_definition.PrimaryKey.GetValue(document).ToString()).Type(_documentTypeName);
        }

        /// <summary>
        /// Format les champs de tri du document.
        /// Les champs de tri sont mis manuellement en minuscule avant indexation.
        /// Ceci est nécessaire car en ElasticSearch 5.x, il n'est plus possible de trier sur un champ indexé (à faible coût).
        /// </summary>
        /// <param name="document">Document.</param>
        /// <returns>Document formaté.</returns>
        private TDocument FormatSortFields(TDocument document)
        {
            foreach (var field in _definition.Fields.Where(x => x.Indexing == SearchFieldIndexing.Sort && x.PropertyType == typeof(string)))
            {
                var raw = field.GetValue(document);
                if (raw != null)
                {
                    field.SetValue(document, ((string)raw).ToLowerInvariant());
                }
            }

            return document;
        }

        /// <summary>
        /// Renvoie le handler de facet pour une définition de facet.
        /// </summary>
        /// <param name="def">Définition de facet.</param>
        /// <returns>Handler.</returns>
        private IFacetHandler<TDocument> GetHandler(IFacetDefinition def)
        {
            return def.GetType() == typeof(PortfolioFacet) ? _portfolioHandler : _standardHandler;
        }

        public ISearchStore<TDocument> RegisterDataSource(string dataSourceName)
        {
            _dataSourceName = dataSourceName ?? throw new ArgumentNullException(nameof(dataSourceName));
            return this;
        }

        /// <summary>
        /// Définition de tri.
        /// </summary>
        private class SortDefinition
        {

            /// <summary>
            /// Ordre de tri.
            /// </summary>
            public SortOrder Order
            {
                get;
                set;
            }

            /// <summary>
            /// Champ du tri (camelCase).
            /// </summary>
            public string FieldName
            {
                get;
                set;
            }

            /// <summary>
            /// Indique si le tri est défini.
            /// </summary>
            public bool HasSort => !string.IsNullOrEmpty(FieldName);
        }
    }
}
