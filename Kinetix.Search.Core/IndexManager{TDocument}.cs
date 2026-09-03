using Microsoft.Extensions.Logging;

namespace Kinetix.Search.Core;

/// <summary>
/// IndexManager pour un document.
/// </summary>
/// <typeparam name="TDocument">Type du document.</typeparam>
public class IndexManager<TDocument>(IIndexManager indexManager)
    where TDocument : class
{
    /// <summary>
    /// Marque un document pour suppression dans son index.
    /// </summary>
    /// <param name="id">ID du document.</param>
    /// <returns>IndexManager.</returns>
    public IndexManager<TDocument> Delete<TKey>(TKey id)
        where TKey : notnull
    {
        indexManager.Delete<TDocument, TKey>(id);
        return this;
    }

    /// <summary>
    /// Marque plusieurs documents pour suppression dans leur index.
    /// </summary>
    /// <param name="ids">IDs des documents.</param>
    /// <returns>IndexManager.</returns>
    public IndexManager<TDocument> DeleteMany<TKey>(IEnumerable<TKey> ids)
        where TKey : notnull
    {
        indexManager.DeleteMany<TDocument, TKey>(ids);
        return this;
    }

    /// <summary>
    /// Marque un document pour (ré)indexation.
    /// </summary>
    /// <param name="id">ID du document.</param>
    /// <returns>IndexManager.</returns>
    public IndexManager<TDocument> Index<TKey>(TKey id)
        where TKey : notnull
    {
        indexManager.Index<TDocument, TKey>(id);
        return this;
    }

    /// <summary>
    /// Réinitialise un index.
    /// </summary>
    /// <returns>this.</returns>
    public IndexManager<TDocument> IndexAll()
    {
        indexManager.IndexAll<TDocument>();
        return this;
    }

    /// <summary>
    /// Marque plusieurs documents pour ré(indexation).
    /// </summary>
    /// <param name="ids">IDs des documents.</param>
    /// <returns>IndexManager.</returns>
    public IndexManager<TDocument> IndexMany<TKey>(IEnumerable<TKey> ids)
        where TKey : notnull
    {
        indexManager.IndexMany<TDocument, TKey>(ids);
        return this;
    }

    /// <summary>
    /// Reconstruit un index.
    /// </summary>
    /// <param name="rebuildLogger">Logger custom pour suivre l'avancement de la réindexation.</param>
    /// <param name="forcePartialRebuild">Force la réindexation partielle, même si l'index n'existait pas avant.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Le nombre de documents.</returns>
    public Task<int> RebuildIndexAsync(
        ILogger? rebuildLogger = null,
        bool forcePartialRebuild = false,
        CancellationToken ct = default
    )
    {
        return indexManager.RebuildIndexAsync<TDocument>(rebuildLogger, forcePartialRebuild, ct);
    }
}
