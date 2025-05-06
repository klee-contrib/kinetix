using Kinetix.Search.Core;
using Kinetix.Search.Core.Querying;
using Kinetix.Search.Models;
using Nest;

namespace Kinetix.Search.Elastic;

/// <summary>
/// Méthodes d'extensions pour le store ES.
/// </summary>
public static class ElasticStoreExtensions
{
    /// <summary>
    /// Effectue une recherche avancée.
    /// </summary>
    /// <param name="store">Store.</param>
    /// <param name="input">Entrée de la recherche.</param>
    /// <param name="documentMapper">Mapper pour convertir le document dans le bon type de sortie.</param>
    /// <param name="filter">Filtre NEST additionnel.</param>
    /// <param name="sorts">Tris NEST additionnels.</param>
    /// <param name="sortsAfter">Si les tris NEST additionnels doivent être après les tris de l'input.</param>
    /// <param name="aggs">Agrégations NEST additionnelles.</param>
    /// <returns>Sortie de la recherche.</returns>
    public static QueryOutput<TOutput> AdvancedQuery<TDocument, TOutput, TCriteria>(this ISearchStore store, AdvancedQueryInput<TDocument, TCriteria> input, Func<TDocument, TOutput> documentMapper, Func<QueryContainerDescriptor<TDocument>, QueryContainer>? filter = null, IEnumerable<Action<SortDescriptor<TDocument>>>? sorts = null, bool sortsAfter = false, Action<AggregationContainerDescriptor<TDocument>>? aggs = null)
        where TDocument : class
        where TCriteria : ICriteria
    {
        return ((ElasticStore)store).AdvancedQuery(input, (d, _) => documentMapper(d), filter, sorts, sortsAfter, aggs);
    }

    /// <summary>
    /// Effectue une recherche avancée.
    /// </summary>
    /// <param name="store">Store.</param>
    /// <param name="input">Entrée de la recherche.</param>
    /// <param name="documentMapper">Mapper pour convertir le document dans le bon type de sortie.</param>
    /// <param name="filter">Filtres NEST additionnel.</param>
    /// <param name="sorts">Tris NEST additionnels.</param>
    /// <param name="sortsAfter">Si les tris NEST additionnels doivent être après les tris de l'input.</param>
    /// <param name="aggs">Agrégations NEST additionnelles.</param>
    /// <returns>Sortie de la recherche.</returns>
    public static QueryOutput<TOutput> AdvancedQuery<TDocument, TOutput, TCriteria>(this ISearchStore store, AdvancedQueryInput<TDocument, TCriteria> input, Func<TDocument, IReadOnlyDictionary<string, IReadOnlyCollection<string>>, TOutput> documentMapper, Func<QueryContainerDescriptor<TDocument>, QueryContainer>? filter = null, IEnumerable<Action<SortDescriptor<TDocument>>>? sorts = null, bool sortsAfter = false, Action<AggregationContainerDescriptor<TDocument>>? aggs = null)
        where TDocument : class
        where TCriteria : ICriteria
    {
        return ((ElasticStore)store).AdvancedQuery(input, documentMapper, filter, sorts, sortsAfter, aggs);
    }

    /// <summary>
    /// Effectue une recherche avancée et récupère tous les résultats (et uniquement les résultats).
    /// </summary>
    /// <param name="store">Store.</param>
    /// <param name="input">Entrée de la recherche.</param>
    /// <param name="documentMapper">Mapper pour convertir le document dans le bon type de sortie.</param>
    /// <param name="filter">Filtre NEST additionnel.</param>
    /// <param name="sorts">Tris NEST additionnels.</param>
    /// <param name="sortsAfter">Si les tris NEST additionnels doivent être après les tris de l'input.</param>
    /// <returns>Résultats.</returns>
    public static IEnumerable<TOutput> AdvancedQueryAll<TDocument, TOutput, TCriteria>(this ISearchStore store, AdvancedQueryInput<TDocument, TCriteria> input, Func<TDocument, TOutput> documentMapper, Func<QueryContainerDescriptor<TDocument>, QueryContainer>? filter = null, IEnumerable<Action<SortDescriptor<TDocument>>>? sorts = null, bool sortsAfter = false)
        where TDocument : class
        where TCriteria : ICriteria
    {
        return ((ElasticStore)store).AdvancedQueryAll(input, (d, _) => documentMapper(d), filter, sorts, sortsAfter);
    }

    /// <summary>
    /// Effectue une recherche avancée et récupère tous les résultats (et uniquement les résultats).
    /// </summary>
    /// <param name="store">Store.</param>
    /// <param name="input">Entrée de la recherche.</param>
    /// <param name="documentMapper">Mapper pour convertir le document dans le bon type de sortie.</param>
    /// <param name="filter">Filtre NEST additionnel.</param>
    /// <param name="sorts">Tris NEST additionnels.</param>
    /// <param name="sortsAfter">Si les tris NEST additionnels doivent être après les tris de l'input.</param>
    /// <returns>Résultats.</returns>
    public static IEnumerable<TOutput> AdvancedQueryAll<TDocument, TOutput, TCriteria>(this ISearchStore store, AdvancedQueryInput<TDocument, TCriteria> input, Func<TDocument, IReadOnlyDictionary<string, IReadOnlyCollection<string>>, TOutput> documentMapper, Func<QueryContainerDescriptor<TDocument>, QueryContainer>? filter = null, IEnumerable<Action<SortDescriptor<TDocument>>>? sorts = null, bool sortsAfter = false)
    where TDocument : class
    where TCriteria : ICriteria
    {
        return ((ElasticStore)store).AdvancedQueryAll(input, documentMapper, filter, sorts, sortsAfter);
    }
}
