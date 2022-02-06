using Kinetix.Search.Models;
using Kinetix.Search.Querying;
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
    /// <param name="input">Entrée de la recherche.</param>
    /// <param name="documentMapper">Mapper pour convertir le document dans le bon type de sortie.</param>
    /// <param name="filters">Filtres NEST additionnels.</param>
    /// <returns>Sortie de la recherche.</returns>
    public static QueryOutput<TOutput> AdvancedQuery<TDocument, TOutput, TCriteria>(this ISearchStore store, AdvancedQueryInput<TDocument, TCriteria> input, Func<TDocument, TOutput> documentMapper, params Func<QueryContainerDescriptor<TDocument>, QueryContainer>[] filters)
        where TDocument : class
        where TCriteria : Criteria, new()
    {
        return ((ElasticStore)store).AdvancedQuery(input, documentMapper, filters);
    }

    /// <summary>
    /// Effectue une recherche avancée et récupère tous les résultats (et uniquement les résultats).
    /// </summary>
    /// <param name="input">Entrée de la recherche.</param>
    /// <param name="documentMapper">Mapper pour convertir le document dans le bon type de sortie.</param>
    /// <param name="filters">Filtres NEST additionnels.</param>
    /// <returns>Résultats.</returns>
    public static IEnumerable<TOutput> AdvancedQueryAll<TDocument, TOutput, TCriteria>(this ISearchStore store, AdvancedQueryInput<TDocument, TCriteria> input, Func<TDocument, TOutput> documentMapper, params Func<QueryContainerDescriptor<TDocument>, QueryContainer>[] filters)
        where TDocument : class
        where TCriteria : Criteria, new()
    {
        return ((ElasticStore)store).AdvancedQueryAll(input, documentMapper, filters);
    }
}
