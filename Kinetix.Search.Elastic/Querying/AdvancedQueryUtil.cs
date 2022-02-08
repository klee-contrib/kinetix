﻿using Kinetix.Search.Core.DocumentModel;
using Kinetix.Search.Models;
using Kinetix.Search.Models.Annotations;
using Kinetix.Search.Core.Querying;
using Nest;

namespace Kinetix.Search.Elastic.Querying;

using static ElasticQueryBuilder;

public static class AdvancedQueryUtil
{
    public const string MissingGroupPrefix = "_Missing";
    public const string TopHitName = "groupTop";

    /// <summary>
    /// Construit le descripteur pour une recherche avancée.
    /// </summary>
    /// <param name="def">Document.</param>
    /// <param name="input">Input de la recherche.</param>
    /// <param name="getFacetHandler">Getter sur le handler de facette.</param>
    /// <param name="facetDefList">Liste des facettes.</param>
    /// <param name="groupFieldName">Nom du champ sur lequel grouper.</param>
    /// <param name="filters">Filtres NEST additionnels.</param>
    /// <returns>Le descripteur.</returns>
    public static Func<SearchDescriptor<TDocument>, ISearchRequest> GetAdvancedQueryDescriptor<TDocument, TCriteria>(
        DocumentDefinition def,
        AdvancedQueryInput<TDocument, TCriteria> input,
        FacetHandler facetHandler,
        Func<QueryContainerDescriptor<TDocument>, QueryContainer>[] filters,
        ICollection<IFacetDefinition<TDocument>> facetDefList = null,
        string groupFieldName = null,
        string pitId = null,
        object[] searchAfter = null)
        where TDocument : class
        where TCriteria : Criteria, new()
    {
        /* Tri */
        var sortDef = GetSortDefinition(def, input);

        /* Requêtes de filtrage. */
        var filterQuery = GetFilterQuery(def, input, facetHandler, filters);
        var (hasPostFilter, postFilterQuery) = GetPostFilterSubQuery(input, facetHandler, def);

        /* Booléens */
        var hasGroup = GetGroupFieldName(input) != null;
        var hasFacet = facetDefList?.Any() ?? false;

        /* Pagination (si plusieurs critères non cohérents, on prend le max). */
        var skip = input.SearchCriteria.Max(sc => sc.Skip);
        var size = hasGroup ? 0 : input.SearchCriteria.Max(sc => sc.Top) ?? 500; // TODO Paramétrable ?

        /* Source filtering */
        var sourceFields = input.SearchCriteria.SelectMany(sc => sc.Criteria.SourceFields ?? Array.Empty<string>()).Distinct().ToArray();

        return (SearchDescriptor<TDocument> s) =>
        {
            s
                /* Critère de filtrage. */
                .Query(filterQuery)

                /* Critère de post-filtrage. */
                .PostFilter(postFilterQuery);

            if (sourceFields.Any())
            {
                s.Source(src => src.Includes(f => f.Fields(sourceFields)));
            }

                /* Pagination */
            if (pitId == null)
            {
                s.From(skip).Size(size).TrackTotalHits();
            }
            else
            {
                s.Size(10000).PointInTime(pitId, p => p.KeepAlive("1m"));

                if (searchAfter != null)
                {
                    s.SearchAfter(searchAfter);
                }
            }

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
                            facetHandler.DefineAggregation(a, facetDef, facetDefList, input.SearchCriteria.Select(sc => sc.Facets));
                        }
                    }
                    if (hasGroup)
                    {
                        AggregationContainerDescriptor<TDocument> AggDescriptor(AggregationContainerDescriptor<TDocument> aa)
                        {
                            return aa
                                /* Groupement. */
                                .Terms(groupFieldName, st => st
                                    .Field(groupFieldName)
                                    .Size(50)
                                    .Aggregations(g => g.TopHits(TopHitName, x => x.Size(input.GroupSize))))
                                /* Groupement pour les valeurs nulles */
                                .Missing(groupFieldName + MissingGroupPrefix, st => st
                                    .Field(groupFieldName)
                                    .Aggregations(g => g.TopHits(TopHitName, x => x.Size(input.GroupSize))));
                        }

                        if (hasPostFilter)
                        {
                                /* Critère de post-filtrage répété sur les groupes, puisque ce sont des agrégations qui par définition ne sont pas affectées par le post-filtrage. */
                            a.Filter(groupFieldName, f => f
                                .Filter(postFilterQuery)
                                .Aggregations(AggDescriptor));
                        }
                        else
                        {
                            AggDescriptor(a);
                        }
                    }

                    return a;
                });
            }

            return s;
        };
    }

    /// <summary>
    /// Récupère la requête de filtrage complète pour l'AdvancedCount.
    /// </summary>
    /// <param name="def">Document.</param>
    /// <param name="input">Input de la recherche.</param>
    /// <param name="getFacetHandler">Getter sur le handler de facette.</param>
    /// <returns></returns>
    public static Func<QueryContainerDescriptor<TDocument>, QueryContainer> GetFilterAndPostFilterQuery<TDocument, TCriteria>(
        DocumentDefinition def,
        AdvancedQueryInput<TDocument, TCriteria> input,
        FacetHandler facetHandler)
        where TDocument : class
        where TCriteria : Criteria, new()
    {
        var (_, postFilterQuery) = GetPostFilterSubQuery(input, facetHandler, def);
        return BuildAndQuery(GetFilterQuery(def, input, facetHandler), postFilterQuery);
    }

    /// <summary>
    /// Créé la requête de filtrage.
    /// </summary>
    /// <param name="def">Document.</param>
    /// <param name="input">Input de la recherche.</param>
    /// <param name="getFacetHandler">Getter sur le handler de facette.</param>
    /// <param name="filters">Filtres NEST additionnels.</param>
    /// <returns>Requête de filtrage.</returns>
    private static Func<QueryContainerDescriptor<TDocument>, QueryContainer> GetFilterQuery<TDocument, TCriteria>(
        DocumentDefinition def,
        AdvancedQueryInput<TDocument, TCriteria> input,
        FacetHandler facetHandler,
        params Func<QueryContainerDescriptor<TDocument>, QueryContainer>[] filters)
        where TDocument : class
        where TCriteria : Criteria, new()
    {
        if (string.IsNullOrWhiteSpace(input.Security))
        {
            input.Security = null;
        }

        if (input.Security != null && def.SecurityField == null)
        {
            throw new ElasticException($@"The Document ""{typeof(TDocument)}"" needs a Security category field to allow Query with security filtering.");
        }

        /* Constuit la sous requête de sécurité. */
        var securitySubQuery = input.Security != null
            ? BuildInclusiveInclude<TDocument>(def.SecurityField.FieldName, input.Security)
            : q => q;

        var isMultiCriteria = input.SearchCriteria.Count() > 1;

        /* Construit la sous requête des différents critères. */
        var criteriaSubQuery = BuildOrQuery(input.SearchCriteria.Select(sc =>
        {
            var criteria = sc.Criteria ?? new TCriteria();

                /* Normalisation des paramètres. */
            if (criteria.Query == "*" || string.IsNullOrWhiteSpace(criteria.Query))
            {
                criteria.Query = null;
            }

                /* Récupération de la liste des champs texte sur lesquels rechercher, potentiellement filtrés par le critère. */
            var searchFields = def.SearchFields
                .Where(sf => criteria.SearchFields == null || criteria.SearchFields.Contains(sf.FieldName))
                .Select(sf => sf.FieldName)
                .ToArray();

                /* Constuit la sous requête de query. */
            var textSubQuery = criteria.Query != null && (criteria.SearchFields?.Any() ?? true)
                ? BuildMultiMatchQuery<TDocument>(criteria.Query, searchFields)
                : q => q;

                /* Gestion des filtres additionnels. */
            var criteriaProperties = typeof(TCriteria).GetProperties();

            var filterList = new List<Func<QueryContainerDescriptor<TDocument>, QueryContainer>>();

            foreach (var field in def.Fields)
            {
                var propName = field.PropertyName;
                var propValue = input.AdditionalCriteria != null
                    ? field.GetValue(input.AdditionalCriteria)
                    : null;

                if (propValue == null)
                {
                    propValue = criteriaProperties.SingleOrDefault(p => p.Name == propName)?.GetValue(sc.Criteria);
                }

                if (propValue != null)
                {
                    var propValueString = propValue switch
                    {
                        bool b => b ? "true" : "false",
                        DateTime d => d.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                        _ => propValue.ToString()
                    };

                    switch (field.Indexing)
                    {
                        case SearchFieldIndexing.FullText:
                            filterList.Add(BuildMultiMatchQuery<TDocument>(propValueString, field.FieldName));
                            break;
                        case SearchFieldIndexing.Term:
                        case SearchFieldIndexing.Sort:
                            filterList.Add(BuildFilter<TDocument>(field.FieldName, propValueString));
                            break;
                        default:
                            throw new ElasticException($"Cannot filter on fields that are not indexed. Field: {field.FieldName}");
                    }
                }
            }

                /* Constuit la sous requête de filtres. */
            var filterSubQuery = BuildAndQuery(filterList.ToArray());

                /* Créé une sous-requête par facette. */
            var facetSubQueryList = sc.Facets
                .Select(f =>
                {
                        /* Récupère la définition de la facette non multi-sélectionnable. */
                    var facetDef = input.FacetQueryDefinition.Facets.Single(x => x.Code == f.Key);
                    if (facetDef.IsMultiSelectable && !isMultiCriteria)
                    {
                        return null;
                    }

                        /* La facette n'est pas multi-sélectionnable donc on prend direct la première valeur (sélectionnée ou exclue). */
                    return facetDef.IsMultiSelectable
                        ? facetHandler.BuildMultiSelectableFilter(f.Value, facetDef, def.Fields[facetDef.FieldName].IsMultiValued)
                        : f.Value.Selected.Any()
                            ? facetHandler.CreateFacetSubQuery(f.Value.Selected.First(), false, facetDef)
                        : f.Value.Excluded.Any()
                            ? facetHandler.CreateFacetSubQuery(f.Value.Excluded.First(), true, facetDef)
                        : null;
                })
                .Where(f => f != null)
                .ToArray();

                /* Concatène en "ET" toutes les sous-requêtes de facettes. */
            var monoValuedFacetsSubQuery = BuildAndQuery(facetSubQueryList);

            return BuildAndQuery(new[] { textSubQuery, filterSubQuery, monoValuedFacetsSubQuery });
        })
        .ToArray());

        return BuildAndQuery(new[] { securitySubQuery, criteriaSubQuery }.Concat(filters).ToArray());
    }

    /// <summary>
    /// Créé la sous-requête de post-filtrage pour les facettes multi-sélectionnables.
    /// </summary>
    /// <param name="input">Input de la recherche.</param>
    /// <param name="getFacetHandler">Getter sur le handler de facette.</param>
    /// <param name="docDef">Document.</param>
    /// <returns>Sous-requête.</returns>
    private static (bool hasPostFilter, Func<QueryContainerDescriptor<TDocument>, QueryContainer> query) GetPostFilterSubQuery<TDocument, TCriteria>(
        AdvancedQueryInput<TDocument, TCriteria> input,
        FacetHandler facetHandler,
        DocumentDefinition docDef)
        where TDocument : class
        where TCriteria : Criteria, new()
    {
        if (input.SearchCriteria.Count() > 1)
        {
            return (false, q => q);
        }

        /* Créé une sous-requête par facette */
        var facetSubQueriesList =
            input.SearchCriteria.Select(sc =>
                sc.Facets.Select(f =>
                {
                        /* Récupère la définition de la facette multi-sélectionnable. */
                    var def = input.FacetQueryDefinition.Facets.SingleOrDefault(x => x.IsMultiSelectable == true && x.Code == f.Key);

                    return def == null
                        ? null
                        : facetHandler.BuildMultiSelectableFilter(f.Value, def, docDef.Fields[def.FieldName].IsMultiValued);
                })
                .Where(f => f != null)
                .ToArray())
            .Where(c => c.Any());

        /* Concatène en "ET" toutes les sous-requêtes. */
        return (
            facetSubQueriesList.Any(),
            BuildOrQuery(facetSubQueriesList.Select(BuildAndQuery).ToArray()));
    }

    /// <summary>
    /// Obtient le nom du champ pour le groupement.
    /// </summary>
    /// <param name="input">Input de la recherche.</param>
    /// <returns>Nom du champ.</returns>
    public static string GetGroupFieldName<TDocument, TCriteria>(AdvancedQueryInput<TDocument, TCriteria> input)
        where TDocument : class
        where TCriteria : Criteria, new()
    {
        // On groupe par le premier groupe renseigné.
        var groupFacetName = input.SearchCriteria.FirstOrDefault(sc => !string.IsNullOrEmpty(sc.Group))?.Group;

        /* Pas de groupement. */
        if (string.IsNullOrEmpty(groupFacetName))
        {
            return null;
        }

        /* Recherche de la facette de groupement. */
        var facetDef = input.FacetQueryDefinition.Facets.SingleOrDefault(x => x.Code == groupFacetName);
        return facetDef == null
            ? throw new ElasticException($@"No facet ""{groupFacetName}"" to group on.")
            : facetDef.FieldName;
    }

    /// <summary>
    /// Obtient la définition du tri.
    /// </summary>
    /// <param name="def">Document.</param>
    /// <param name="input">Input de la recherche.</param>
    /// <returns>Définition du tri.</returns>
    private static SortDefinition GetSortDefinition<TDocument, TCriteria>(
        DocumentDefinition def,
        AdvancedQueryInput<TDocument, TCriteria> input)
        where TDocument : class
        where TCriteria : Criteria, new()
    {
        // On trie par le premier tri renseigné.
        var fieldName = input.SearchCriteria.FirstOrDefault(sc => !string.IsNullOrEmpty(sc.SortFieldName))?.SortFieldName;

        /* Cas de l'absence de tri. */
        if (string.IsNullOrEmpty(fieldName))
        {
            // On trie "au hasard" par le premier champ indexé "Sort" qu'on a trouvé.
            fieldName = def.Fields.FirstOrDefault(field => field.Indexing == SearchFieldIndexing.Sort)?.FieldName;

            // Si y en a vraiment pas, on trie pas.
            if (fieldName == null)
            {
                return new SortDefinition();
            }
        }

        /* Vérifie la présence du champ. */
        return !def.Fields.HasProperty(fieldName)
            ? throw new ElasticException($@"The Document ""{typeof(TDocument)}"" is missing a ""{fieldName}"" property to sort on.")
            : new SortDefinition
            {
                FieldName = def.Fields[fieldName].FieldName,

                    // Seul le premier ordre est utilisé.
                    Order = input.SearchCriteria.First().SortDesc ? SortOrder.Descending : SortOrder.Ascending
            };
    }

    /// <summary>
    /// Définition de tri.
    /// </summary>
    public class SortDefinition
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
