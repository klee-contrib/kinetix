using Kinetix.Search.Core.Querying;
using Kinetix.Search.Models;

namespace Kinetix.Search.Core;

/// <summary>
/// Extensions pour ISearchStore.
/// </summary>
public static class SearchStoreExtensions
{
    /// <summary>
    /// Instancie un IndexManager pour le document demandé.
    /// </summary>
    /// <typeparam name="TDocument">Type de document.</typeparam>
    /// <returns>IndexManager.</returns>
    public static IndexManager<TDocument> For<TDocument>(this IIndexManager indexManager)
        where TDocument : class
    {
        return new IndexManager<TDocument>(indexManager);
    }

    /// <summary>
    /// Effectue une requête sur le champ texte.
    /// </summary>
    /// <param name="store">Store de recherche.</param>
    /// <param name="queryInput">Query input.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Résultat.</returns>
    public static Task<(IEnumerable<TDocument> Data, int TotalCount)> QueryAsync<TDocument>(
        this ISearchStore store,
        BasicQueryInput<TDocument> queryInput,
        CancellationToken ct = default
    )
        where TDocument : class
    {
        return store.QueryAsync(queryInput, x => x, ct: ct);
    }

    /// <summary>
    /// Effectue une requête sur le champ texte.
    /// </summary>
    /// <param name="store">Store de recherche.</param>
    /// <param name="queryInput">Query input.</param>
    /// <param name="documentMapper">Mapper de document.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Résultat.</returns>
    public static Task<(IEnumerable<TOutput> Data, int TotalCount)> QueryAsync<TDocument, TOutput>(
        this ISearchStore store,
        BasicQueryInput<TDocument> queryInput,
        Func<TDocument, TOutput> documentMapper,
        CancellationToken ct = default
    )
        where TDocument : class
    {
        return store.QueryAsync(queryInput, (d, _) => documentMapper(d), ct: ct);
    }

    /// <summary>
    /// Effectue une requête sur le champ texte.
    /// </summary>
    /// <param name="store">Store de recherche.</param>
    /// <param name="queryInput">Query input.</param>
    /// <param name="documentMapper">Mapper de document.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Résultat.</returns>
    public static Task<(IEnumerable<TOutput> Data, int TotalCount)> QueryAsync<TDocument, TOutput>(
        this ISearchStore store,
        BasicQueryInput<TDocument> queryInput,
        Func<TDocument, IReadOnlyDictionary<string, IReadOnlyCollection<string>>, TOutput> documentMapper,
        CancellationToken ct = default
    )
        where TDocument : class
    {
        return store.QueryAsync(queryInput, new DefaultCriteria { Query = queryInput.Query }, documentMapper, ct: ct);
    }

    /// <summary>
    /// Effectue une requête sur le champ texte.
    /// </summary>
    /// <param name="store">Store de recherche.</param>
    /// <param name="criteria">Critère de recherche.</param>
    /// <param name="documentMapper">Mapper de document.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Résultat.</returns>
    public static Task<(IEnumerable<TOutput> Data, int TotalCount)> QueryAsync<TDocument, TCriteria, TOutput>(
        this ISearchStore store,
        TCriteria criteria,
        Func<TDocument, TOutput> documentMapper,
        CancellationToken ct = default
    )
        where TDocument : class
        where TCriteria : ICriteria
    {
        return store.QueryAsync(queryInput: null, criteria, documentMapper, ct: ct);
    }

    /// <summary>
    /// Effectue une requête sur le champ texte.
    /// </summary>
    /// <param name="store">Store de recherche.</param>
    /// <param name="criteria">Critère de recherche.</param>
    /// <param name="documentMapper">Mapper de document.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Résultat.</returns>
    public static Task<(IEnumerable<TOutput> Data, int TotalCount)> QueryAsync<TDocument, TCriteria, TOutput>(
        this ISearchStore store,
        TCriteria criteria,
        Func<TDocument, IReadOnlyDictionary<string, IReadOnlyCollection<string>>, TOutput> documentMapper,
        CancellationToken ct = default
    )
        where TDocument : class
        where TCriteria : ICriteria
    {
        return store.QueryAsync(queryInput: null, criteria, documentMapper, ct: ct);
    }

    /// <summary>
    /// Effectue une requête sur le champ texte.
    /// </summary>
    /// <param name="store">Store de recherche.</param>
    /// <param name="queryInput">Query input.</param>
    /// <param name="criteria">Critère de recherche.</param>
    /// <param name="documentMapper">Mapper de document.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Résultat.</returns>
    public static Task<(IEnumerable<TOutput> Data, int TotalCount)> QueryAsync<TDocument, TCriteria, TOutput>(
        this ISearchStore store,
        BasicQueryInput<TDocument>? queryInput,
        TCriteria criteria,
        Func<TDocument, TOutput> documentMapper,
        CancellationToken ct = default
    )
        where TDocument : class
        where TCriteria : ICriteria
    {
        return store.QueryAsync(queryInput, criteria, (d, _) => documentMapper(d), ct: ct);
    }

    /// <summary>
    /// Effectue une requête sur le champ texte.
    /// </summary>
    /// <param name="store">Store de recherche.</param>
    /// <param name="queryInput">Query input.</param>
    /// <param name="criteria">Critère de recherche.</param>
    /// <param name="documentMapper">Mapper de document.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Résultat.</returns>
    public static async Task<(IEnumerable<TOutput> Data, int TotalCount)> QueryAsync<TDocument, TCriteria, TOutput>(
        this ISearchStore store,
        BasicQueryInput<TDocument>? queryInput,
        TCriteria criteria,
        Func<TDocument, IReadOnlyDictionary<string, IReadOnlyCollection<string>>, TOutput> documentMapper,
        CancellationToken ct = default
    )
        where TDocument : class
        where TCriteria : ICriteria
    {
        if (string.IsNullOrEmpty(criteria.Query))
        {
            return (new List<TOutput>(), 0);
        }

        var input = new AdvancedQueryInput<TDocument, TCriteria>
        {
            SearchCriteria =
            [
                new QueryInput<TCriteria>
                {
                    Criteria = criteria,
                    Skip = 0,
                    Top = queryInput?.Top ?? 10,
                },
            ],
            Security = queryInput?.Security,
            AdditionalCriteria = queryInput?.Criteria,
        };

        var output = await store.AdvancedQueryAsync(input, documentMapper, ct);
        return (output.List!, output.TotalCount);
    }
}
