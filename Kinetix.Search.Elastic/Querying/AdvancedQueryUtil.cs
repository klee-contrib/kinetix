using System;
using System.Collections.Generic;
using System.Linq;
using Kinetix.Search.Attributes;
using Kinetix.Search.ComponentModel;
using Kinetix.Search.MetaModel;
using Kinetix.Search.Model;
using Nest;

namespace Kinetix.Search.Elastic.Querying
{
    using static ElasticQueryBuilder;

    public static class AdvancedQueryUtil
    {
        public const string MissingGroupPrefix = "_Missing";
        public const string TopHitName = "top";

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
            ICollection<IFacetDefinition<TDocument>> facetDefList,
            string groupFieldName,
            Func<QueryContainerDescriptor<TDocument>, QueryContainer>[] filters)
            where TDocument : class
            where TCriteria : Criteria, new()
        {
            /* Tri */
            var sortDef = GetSortDefinition(def, input);

            /* Requêtes de filtrage. */
            var filterQuery = GetFilterQuery(def, input, facetHandler, filters);
            var (hasPostFilter, postFilterQuery) = GetPostFilterSubQuery(input, facetHandler, def);

            /* Booléens */
            var hasGroup = !string.IsNullOrEmpty(input.ApiInput.Group);
            var hasFacet = facetDefList.Any();

            /* Pagination. */
            var skip = input.ApiInput.Skip;
            var size = hasGroup ? 0 : input.ApiInput.Top ?? 500; // TODO Paramétrable ?

            return (SearchDescriptor<TDocument> s) =>
            {
                s
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
                                facetHandler.DefineAggregation(a, facetDef, facetDefList, input.ApiInput.Facets);
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
            var criteria = input.ApiInput.Criteria ?? new TCriteria();

            /* Normalisation des paramètres. */
            if (criteria.Query == "*" || string.IsNullOrWhiteSpace(criteria.Query))
            {
                criteria.Query = null;
            }

            if (string.IsNullOrWhiteSpace(input.Security))
            {
                input.Security = null;
            }

            /* Récupération de la liste des champs texte sur lesquels rechercher, potentiellement filtrés par le critère. */
            var searchFields = def.SearchFields
                .Where(sf => criteria.SearchFields == null || criteria.SearchFields.Contains(sf.FieldName))
                .Select(sf => sf.FieldName)
                .ToArray();

            if (input.Security != null && def.SecurityField == null)
            {
                throw new ElasticException($@"The Document ""{typeof(TDocument)}"" needs a Security category field to allow Query with security filtering.");
            }

            /* Constuit la sous requête de query. */
            var textSubQuery = criteria.Query != null && (criteria.SearchFields?.Any() ?? true)
                ? BuildMultiMatchQuery<TDocument>(criteria.Query, searchFields)
                : q => q;

            /* Constuit la sous requête de sécurité. */
            var securitySubQuery = input.Security != null
                ? BuildInclusiveInclude<TDocument>(def.SecurityField.FieldName, input.Security)
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
                    propValue = criteriaProperties.SingleOrDefault(p => p.Name == propName)?.GetValue(input.ApiInput.Criteria);
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
            var facetSubQueryList = input.ApiInput.Facets
                .Select(f =>
                {
                    /* Récupère la définition de la facette non multi-sélectionnable. */
                    var facetDef = input.FacetQueryDefinition.Facets.SingleOrDefault(x => x.IsMultiSelectable == false && x.Code == f.Key);
                    if (facetDef == null)
                    {
                        return null;
                    }

                    /* La facette n'est pas multi-sélectionnable donc on prend direct la première valeur (sélectionnée ou exclue). */
                    return
                        f.Value.Selected.Any()
                            ? facetHandler.CreateFacetSubQuery<TDocument>(f.Value.Selected.First(), false, facetDef)
                        : f.Value.Excluded.Any()
                            ? facetHandler.CreateFacetSubQuery<TDocument>(f.Value.Excluded.First(), true, facetDef)
                        : null;
                })
                .Where(f => f != null)
                .ToArray();

            /* Concatène en "ET" toutes les sous-requêtes de facettes. */
            var monoValuedFacetsSubQuery = facetSubQueryList.Any()
                ? BuildAndQuery(facetSubQueryList)
                : q => q;

            return BuildAndQuery(
                new[] { textSubQuery, securitySubQuery, filterSubQuery, monoValuedFacetsSubQuery }
                .Concat(filters)
                .ToArray());
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
            /* Créé une sous-requête par facette */
            var facetSubQueryList = input.ApiInput.Facets
                .Select(f =>
                {
                    /* Récupère la définition de la facette multi-sélectionnable. */
                    var def = input.FacetQueryDefinition.Facets.SingleOrDefault(x => x.IsMultiSelectable == true && x.Code == f.Key);

                    return def == null
                        ? null
                        : facetHandler.BuildMultiSelectableFilter<TDocument>(f.Value, def, docDef.Fields[def.FieldName].IsMultiValued);
                })
                .Where(f => f != null)
                .ToArray();

            /* Concatène en "ET" toutes les sous-requêtes. */
            return (
                facetSubQueryList.Any(),
                facetSubQueryList.Any()
                    ? BuildAndQuery(facetSubQueryList)
                    : q => q);
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
            var groupFacetName = input.ApiInput.Group;

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
            var fieldName = input.ApiInput.SortFieldName;

            /* Cas de l'absence de tri. */
            if (string.IsNullOrEmpty(fieldName))
            {
                return new SortDefinition();
            }

            /* Vérifie la présence du champ. */
            return !def.Fields.HasProperty(fieldName)
                ? throw new ElasticException($@"The Document ""{typeof(TDocument)}"" is missing a ""{fieldName}"" property to sort on.")
                : new SortDefinition
                {
                    FieldName = def.Fields[fieldName].FieldName,
                    Order = input.ApiInput.SortDesc ? SortOrder.Descending : SortOrder.Ascending
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
}
