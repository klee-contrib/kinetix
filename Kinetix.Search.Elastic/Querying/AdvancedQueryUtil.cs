#pragma warning disable S3241

using Kinetix.Search.Core.DocumentModel;
using Kinetix.Search.Core.Querying;
using Kinetix.Search.Models;
using Kinetix.Search.Models.Annotations;
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
    /// <param name="facetHandler">Handler de facette.</param>
    /// <param name="filter">Filtre NEST additionnel.</param>
    /// <param name="sorts">Tris NEST additionnels.</param>
    /// <param name="sortsAfter">Si les tris NEST additionnels doivent être après les tris de l'input.</param>
    /// <param name="aggs">Agrégations NEST additionnelles.</param>
    /// <param name="facetDefList">Liste des facettes.</param>
    /// <param name="groupFieldName">Nom du champ sur lequel grouper.</param>
    /// <param name="pitId">Id du PIT, si recheche paginée.</param>
    /// <param name="searchAfter">Id du dernier élément retourné, si paginé.</param>
    /// <returns>Le descripteur.</returns>
    public static Func<SearchDescriptor<TDocument>, ISearchRequest> GetAdvancedQueryDescriptor<TDocument, TCriteria>(
        DocumentDefinition def,
        AdvancedQueryInput<TDocument, TCriteria> input,
        FacetHandler facetHandler,
        Func<QueryContainerDescriptor<TDocument>, QueryContainer>? filter = null,
        IEnumerable<Action<SortDescriptor<TDocument>>>? sorts = null,
        bool sortsAfter = false,
        Action<AggregationContainerDescriptor<TDocument>>? aggs = null,
        ICollection<IFacetDefinition<TDocument>>? facetDefList = null,
        string? groupFieldName = null,
        string? pitId = null,
        object[]? searchAfter = null
    )
        where TDocument : class
        where TCriteria : ICriteria
    {
        /* Tri */
        var sortDefs = GetSortDefinitions(def, input);
        if (sorts != null)
        {
            sortDefs = sortsAfter ? sortDefs.Concat(sorts) : sorts.Concat(sortDefs);
        }

        /* Requêtes de filtrage. */
        var filterQuery = GetFilterQuery(def, input, filter);
        var (hasPostFilter, postFilterQuery) = GetPostFilterSubQuery(input, def);

        /* Booléens */
        var hasGroup = GetGroupFieldName(input) != null;
        var hasFacet = facetDefList?.Any() ?? false;

        /* Pagination (si plusieurs critères non cohérents, on prend le max). */
        var skip = input.SearchCriteria.Max(sc => sc.Skip);
        var size = hasGroup ? 0 : input.SearchCriteria.Max(sc => sc.Top) ?? 500;

        /* Source filtering */
        var sourceFields = input
            .SearchCriteria.SelectMany(sc => sc.Criteria?.SourceFields ?? Array.Empty<string>())
            .Distinct()
            .ToArray();

        return s =>
        {
            s
            /* Critère de filtrage. */
            .Query(filterQuery)
                /* Critère de post-filtrage. */
                .PostFilter(postFilterQuery);

            if (sourceFields.Length != 0)
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
            s.Sort(x =>
            {
                if (sortDefs.Any())
                {
                    foreach (var sortDef in sortDefs)
                    {
                        sortDef(x);
                    }
                }
                else
                {
                    x.Field("_score", SortOrder.Descending);
                }

                return x;
            });

            IHighlight HighlightSelector(HighlightDescriptor<TDocument> h) =>
                h.Fields(
                    def.SearchFields.Select(f =>
                            (Func<HighlightFieldDescriptor<TDocument>, IHighlightField>)(h => h.Field(f.FieldName))
                        )
                        .ToArray()
                );

            /* Aggrégations. */
            if (hasFacet || hasGroup)
            {
                s.Aggregations(a =>
                {
                    if (hasFacet && facetDefList != null)
                    {
                        /* Facettage. */
                        foreach (var facetDef in facetDefList)
                        {
                            facetHandler.DefineAggregation(
                                a,
                                facetDef,
                                facetDefList,
                                input.SearchCriteria.Select(sc => sc.Facets)
                            );
                        }
                    }

                    if (hasGroup)
                    {
                        AggregationContainerDescriptor<TDocument> AggDescriptor(
                            AggregationContainerDescriptor<TDocument> aa
                        )
                        {
                            return aa
                            /* Groupement. */
                            .Terms(
                                    groupFieldName,
                                    st =>
                                        st.Field(groupFieldName)
                                            .Size(50)
                                            .Order(t =>
                                                facetDefList
                                                    ?.FirstOrDefault(f => f.FieldName == groupFieldName)
                                                    ?.Ordering switch
                                                {
                                                    FacetOrdering.KeyAscending => t.KeyAscending(),
                                                    FacetOrdering.KeyDescending => t.KeyDescending(),
                                                    FacetOrdering.CountAscending => t.CountAscending(),
                                                    _ => t.CountDescending(),
                                                }
                                            )
                                            .Aggregations(g =>
                                                g.TopHits(
                                                    TopHitName,
                                                    x =>
                                                    {
                                                        x.Size(input.GroupSize)
                                                            .Sort(x =>
                                                            {
                                                                if (sortDefs.Any())
                                                                {
                                                                    foreach (var sortDef in sortDefs)
                                                                    {
                                                                        sortDef(x);
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    x.Field("_score", SortOrder.Descending);
                                                                }

                                                                return x;
                                                            });

                                                        if (input.Highlights)
                                                        {
                                                            x.Highlight(HighlightSelector);
                                                        }

                                                        return x;
                                                    }
                                                )
                                            )
                                )
                                /* Groupement pour les valeurs nulles */
                                .Missing(
                                    groupFieldName + MissingGroupPrefix,
                                    st =>
                                        st.Field(groupFieldName)
                                            .Aggregations(g =>
                                                g.TopHits(
                                                    TopHitName,
                                                    x =>
                                                    {
                                                        x.Size(input.GroupSize);

                                                        if (input.Highlights)
                                                        {
                                                            x.Highlight(HighlightSelector);
                                                        }

                                                        return x;
                                                    }
                                                )
                                            )
                                );
                        }

                        if (hasPostFilter)
                        {
                            /* Critère de post-filtrage répété sur les groupes, puisque ce sont des agrégations qui par définition ne sont pas affectées par le post-filtrage. */
                            a.Filter(groupFieldName, f => f.Filter(postFilterQuery).Aggregations(AggDescriptor));
                        }
                        else
                        {
                            AggDescriptor(a);
                        }
                    }

                    aggs?.Invoke(a);

                    return a;
                });
            }

            if (input.Highlights)
            {
                s.Highlight(HighlightSelector);
            }

            return s;
        };
    }

    /// <summary>
    /// Récupère la requête de filtrage complète pour l'AdvancedCount.
    /// </summary>
    /// <param name="def">Document.</param>
    /// <param name="input">Input de la recherche.</param>
    /// <returns>QueryDescriptor.</returns>
    public static Func<QueryContainerDescriptor<TDocument>, QueryContainer> GetFilterAndPostFilterQuery<
        TDocument,
        TCriteria
    >(DocumentDefinition def, AdvancedQueryInput<TDocument, TCriteria> input)
        where TDocument : class
        where TCriteria : ICriteria
    {
        var (_, postFilterQuery) = GetPostFilterSubQuery(input, def);
        return BuildMustQuery(GetFilterQuery(def, input), postFilterQuery);
    }

    /// <summary>
    /// Obtient le nom du champ pour le groupement.
    /// </summary>
    /// <param name="input">Input de la recherche.</param>
    /// <returns>Nom du champ.</returns>
    public static string? GetGroupFieldName<TDocument, TCriteria>(AdvancedQueryInput<TDocument, TCriteria> input)
        where TDocument : class
        where TCriteria : ICriteria
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
    /// Créé la requête de filtrage.
    /// </summary>
    /// <param name="def">Document.</param>
    /// <param name="input">Input de la recherche.</param>
    /// <param name="filter">Filtre NEST additionnel.</param>
    /// <returns>Requête de filtrage.</returns>
    private static Func<QueryContainerDescriptor<TDocument>, QueryContainer> GetFilterQuery<TDocument, TCriteria>(
        DocumentDefinition def,
        AdvancedQueryInput<TDocument, TCriteria> input,
        Func<QueryContainerDescriptor<TDocument>, QueryContainer>? filter = null
    )
        where TDocument : class
        where TCriteria : ICriteria
    {
        if (input.Security?.Length == 0)
        {
            input.Security = null;
        }

        if (input.Security != null && def.SecurityField == null)
        {
            throw new ElasticException(
                $@"The Document ""{typeof(TDocument)}"" needs a Security category field to allow Query with security filtering."
            );
        }

        /* Constuit la sous requête de sécurité. */
        var securitySubQuery =
            input.Security != null
                ? BuildOrQuery(
                    input.Security.Select(s => BuildFilter<TDocument>(def.SecurityField!.FieldName, s)).ToArray()
                )
                : q => q;

        var isMultiCriteria = input.SearchCriteria.Count() > 1;

        /* Construit la sous requête des différents critères. */
        var criteriaSubQuery = BuildOrQuery(
            input
                .SearchCriteria.Select(sc =>
                {
                    var criteria = sc.Criteria;

                    /* Normalisation des paramètres. */
                    if (criteria is not null && (criteria.Query == "*" || string.IsNullOrWhiteSpace(criteria.Query)))
                    {
                        criteria.Query = null;
                    }

                    /* Récupération de la liste des champs texte sur lesquels rechercher, potentiellement filtrés par le critère. */
                    var searchFields = def
                        .SearchFields.Where(sf =>
                            criteria?.SearchFields == null || criteria.SearchFields.Contains(sf.FieldName)
                        )
                        .ToArray();

                    /* Constuit la sous requête de query. */
                    var textSubQuery =
                        criteria?.Query != null && (criteria.SearchFields?.Any() ?? true)
                            ? BuildMultiMatchQuery<TDocument>(criteria.Query, searchFields)
                            : q => q;

                    /* Gestion des filtres additionnels. */
                    var criteriaProperties = typeof(TCriteria).GetProperties();

                    var filterList = new List<Func<QueryContainerDescriptor<TDocument>, QueryContainer>>();

                    foreach (var field in def.Fields)
                    {
                        var propName = field.PropertyName;
                        var propValue =
                            input.AdditionalCriteria != null ? field.GetValue(input.AdditionalCriteria) : null;

                        if (sc.Criteria is not null)
                        {
                            propValue ??= criteriaProperties
                                .SingleOrDefault(p => p.Name == propName)
                                ?.GetValue(sc.Criteria);
                        }

                        if (propValue != null)
                        {
                            var propValueString = propValue switch
                            {
                                bool b => b ? "true" : "false",
                                DateTime d => d.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                                _ => propValue.ToString()!,
                            };

                            switch (field.Indexing)
                            {
                                case SearchFieldIndexing.FullText:
                                    filterList.Add(BuildMultiMatchQuery<TDocument>(propValueString, field));
                                    break;
                                case SearchFieldIndexing.Term:
                                case SearchFieldIndexing.Sort:
                                    filterList.Add(BuildFilter<TDocument>(field.FieldName, propValueString));
                                    break;
                                default:
                                    throw new ElasticException(
                                        $"Cannot filter on fields that are not indexed. Field: {field.FieldName}"
                                    );
                            }
                        }
                    }

                    /* Constuit la sous requête de filtres. */
                    var filterSubQuery = BuildAndQuery(filterList.ToArray());

                    /* Créé une sous-requête par facette. */
                    var facetSubQueryList = sc
                        .Facets.Select(f =>
                        {
                            /* Récupère la définition de la facette non multi-sélectionnable. */
                            var facetDef = input.FacetQueryDefinition.Facets.Single(x => x.Code == f.Key);
                            if (facetDef.IsMultiSelectable && !isMultiCriteria)
                            {
                                return null!;
                            }

                            /* La facette n'est pas multi-sélectionnable donc on prend direct la première valeur (sélectionnée ou exclue). */
                            return facetDef.IsMultiSelectable
                                    ? FacetHandler.BuildMultiSelectableFilter(
                                        f.Value,
                                        facetDef,
                                        def.Fields[facetDef.FieldName].IsMultiValued
                                    )!
                                : f.Value.Selected.Any()
                                    ? FacetHandler.CreateFacetSubQuery(f.Value.Selected[0], exclude: false, facetDef)
                                : f.Value.Excluded.Any()
                                    ? FacetHandler.CreateFacetSubQuery(f.Value.Excluded[0], exclude: true, facetDef)
                                : null!;
                        })
                        .Where(f => f != null)
                        .ToArray();

                    /* Concatène en "ET" toutes les sous-requêtes de facettes. */
                    var monoValuedFacetsSubQuery = BuildAndQuery(facetSubQueryList);

                    return BuildMustQuery(textSubQuery, filterSubQuery, monoValuedFacetsSubQuery);
                })
                .ToArray()
        );

        return BuildMustQuery(new[] { securitySubQuery, criteriaSubQuery, filter! }.Where(f => f != null).ToArray());
    }

    /// <summary>
    /// Créé la sous-requête de post-filtrage pour les facettes multi-sélectionnables.
    /// </summary>
    /// <param name="input">Input de la recherche.</param>
    /// <param name="docDef">Document.</param>
    /// <returns>Sous-requête.</returns>
    private static (
        bool HasPostFilter,
        Func<QueryContainerDescriptor<TDocument>, QueryContainer> Query
    ) GetPostFilterSubQuery<TDocument, TCriteria>(
        AdvancedQueryInput<TDocument, TCriteria> input,
        DocumentDefinition docDef
    )
        where TDocument : class
        where TCriteria : ICriteria
    {
        if (input.SearchCriteria.Count() > 1)
        {
            return (false, q => q);
        }

        /* Créé une sous-requête par facette */
        var facetSubQueriesList = input
            .SearchCriteria.Select(sc =>
                sc.Facets.Select(f =>
                    {
                        /* Récupère la définition de la facette multi-sélectionnable. */
                        var def = input.FacetQueryDefinition.Facets.SingleOrDefault(x =>
                            x.IsMultiSelectable && x.Code == f.Key
                        );

                        return def == null
                            ? null!
                            : FacetHandler.BuildMultiSelectableFilter(
                                f.Value,
                                def,
                                docDef.Fields[def.FieldName].IsMultiValued
                            )!;
                    })
                    .Where(f => f != null)
                    .ToArray()
            )
            .Where(c => c.Length != 0);

        /* Concatène en "ET" toutes les sous-requêtes. */
        return (facetSubQueriesList.Any(), BuildOrQuery(facetSubQueriesList.Select(BuildAndQuery).ToArray()));
    }

    /// <summary>
    /// Obtient la définition du tri.
    /// </summary>
    /// <param name="def">Document.</param>
    /// <param name="input">Input de la recherche.</param>
    /// <returns>Définition du tri.</returns>
    private static IEnumerable<Action<SortDescriptor<TDocument>>> GetSortDefinitions<TDocument, TCriteria>(
        DocumentDefinition def,
        AdvancedQueryInput<TDocument, TCriteria> input
    )
        where TDocument : class
        where TCriteria : ICriteria
    {
        // On trie par le premier tri renseigné.
        var criteria = input.SearchCriteria.FirstOrDefault(sc =>
            !string.IsNullOrEmpty(sc.SortFieldName) || sc.Sort.Count > 0
        );
        if (criteria == null)
        {
            yield break;
        }

        var sorts =
            criteria.Sort.Count > 0
                ? criteria.Sort
                : [new() { FieldName = criteria.SortFieldName!, SortDesc = criteria.SortDesc }];

        foreach (var sort in sorts)
        {
            if (!def.Fields.HasProperty(sort.FieldName))
            {
                throw new ElasticException(
                    $@"The Document ""{typeof(TDocument)}"" is missing a ""{sort.FieldName}"" property to sort on."
                );
            }

            yield return x =>
                x.Field(
                    def.Fields[sort.FieldName].FieldName,
                    sort.SortDesc ? SortOrder.Descending : SortOrder.Ascending
                );
        }
    }
}
