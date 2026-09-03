using Kinetix.Search.Core.Querying;
using Kinetix.Search.Models;
using Microsoft.Extensions.Logging;

namespace Kinetix.Search.Core;

/// <summary>
/// Contrat des stores de recherche.
/// </summary>
public interface ISearchStore
{
    /// <summary>
    /// Effectue un count avancé.
    /// </summary>
    /// <param name="input">Entrée de la recherche.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Nombre de documents.</returns>
    Task<long> AdvancedCountAsync<TDocument, TCriteria>(
        AdvancedQueryInput<TDocument, TCriteria> input,
        CancellationToken ct = default
    )
        where TDocument : class
        where TCriteria : ICriteria;

    /// <summary>
    /// Effectue une recherche avancée.
    /// </summary>
    /// <param name="input">Entrée de la recherche.</param>
    /// <param name="documentMapper">Mapper pour convertir le document dans le bon type de sortie.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Sortie de la recherche.</returns>
    Task<QueryOutput<TOutput>> AdvancedQueryAsync<TDocument, TOutput, TCriteria>(
        AdvancedQueryInput<TDocument, TCriteria> input,
        Func<TDocument, TOutput> documentMapper,
        CancellationToken ct = default
    )
        where TDocument : class
        where TCriteria : ICriteria;

    /// <summary>
    /// Effectue une recherche avancée.
    /// </summary>
    /// <param name="input">Entrée de la recherche.</param>
    /// <param name="documentMapper">Mapper pour convertir le document dans le bon type de sortie.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Sortie de la recherche.</returns>
    Task<QueryOutput<TOutput>> AdvancedQueryAsync<TDocument, TOutput, TCriteria>(
        AdvancedQueryInput<TDocument, TCriteria> input,
        Func<TDocument, IReadOnlyDictionary<string, IReadOnlyCollection<string>>, TOutput> documentMapper,
        CancellationToken ct = default
    )
        where TDocument : class
        where TCriteria : ICriteria;

    /// <summary>
    /// Permet d'effectuer des indexations et de suppressions en masse.
    /// </summary>
    /// <returns>ISearchBulkDescriptor.</returns>
    ISearchBulkDescriptor Bulk();

    /// <summary>
    /// Supprime un document de l'index.
    /// </summary>
    /// <param name="id">ID du document.</param>
    /// <param name="refresh">Attends ou non la réindexation.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Task.</returns>
    Task DeleteAsync<TDocument>(object id, bool refresh = true, CancellationToken ct = default)
        where TDocument : class;

    /// <summary>
    /// S'assure que l'index existe, avec le mapping à jour.
    /// </summary>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>True si l'index a été (re)créé.</returns>
    Task<bool> EnsureIndexAsync<TDocument>(CancellationToken ct = default)
        where TDocument : class;

    /// <summary>
    /// Obtient un document à partir de son ID.
    /// </summary>
    /// <param name="id">ID du document.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Document.</returns>
    Task<TDocument> GetAsync<TDocument>(object id, CancellationToken ct = default)
        where TDocument : class;

    /// <summary>
    /// Pose un document dans l'index.
    /// </summary>
    /// <param name="document">Document à poser.</param>
    /// <param name="refresh">Attends ou non la réindexation.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Task.</returns>
    Task IndexAsync<TDocument>(TDocument document, bool refresh = true, CancellationToken ct = default)
        where TDocument : class;

    /// <summary>
    /// Effectue une recherche avancée mutiple.
    /// </summary>
    /// <returns>Descripteur.</returns>
    IMultiAdvancedQueryDescriptor MultiAdvancedQuery();

    /// <summary>
    /// Réinitialise l'index avec les documents fournis.
    /// </summary>
    /// <param name="documents">Documents.</param>
    /// <param name="partialRebuild">Reconstruction partielle (si un index à jour existe déjà).</param>
    /// <param name="rebuildLogger">Logger custom pour suivre l'avancement de la réindexation.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Le nombre de documents.</returns>
    Task<int> ResetIndexAsync<TDocument>(
        IAsyncEnumerable<TDocument> documents,
        bool partialRebuild,
        ILogger? rebuildLogger = null,
        CancellationToken ct = default
    )
        where TDocument : class;
}
