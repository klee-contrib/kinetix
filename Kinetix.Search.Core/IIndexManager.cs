using Microsoft.Extensions.Logging;

namespace Kinetix.Search.Core;

/// <summary>
/// Gestionnaire de réindexation des documents.
/// </summary>
public interface IIndexManager
{
    /// <summary>
    /// Attends le refresh de l'index lors du commit ou non. Par défaut: true.
    /// </summary>
    bool WaitForRefresh { get; set; }

    /// <summary>
    /// Marque un document pour suppression dans son index.
    /// </summary>
    /// <param name="id">ID du document.</param>
    /// <returns>IndexManager.</returns>
    IIndexManager Delete<TDocument, TKey>(TKey id)
        where TDocument : class
        where TKey : notnull;

    /// <summary>
    /// Marque plusieurs documents pour suppression dans leur index.
    /// </summary>
    /// <param name="ids">IDs des documents.</param>
    /// <returns>IndexManager.</returns>
    IIndexManager DeleteMany<TDocument, TKey>(IEnumerable<TKey> ids)
        where TDocument : class
        where TKey : notnull;

    /// <summary>
    /// Marque un document pour (ré)indexation.
    /// </summary>
    /// <param name="id">ID du document.</param>
    /// <returns>IndexManager.</returns>
    IIndexManager Index<TDocument, TKey>(TKey id)
        where TDocument : class
        where TKey : notnull;

    /// <summary>
    /// Réinitialise un index.
    /// </summary>
    /// <returns>this.</returns>
    IIndexManager IndexAll<TDocument>()
        where TDocument : class;

    /// <summary>
    /// Marque plusieurs documents pour ré(indexation).
    /// </summary>
    /// <param name="ids">IDs des documents.</param>
    /// <returns>IndexManager.</returns>
    IIndexManager IndexMany<TDocument, TKey>(IEnumerable<TKey> ids)
        where TDocument : class
        where TKey : notnull;

    /// <summary>
    /// Reconstruit un index.
    /// </summary>
    /// <param name="rebuildLogger">Logger custom pour suivre l'avancement de la réindexation.</param>
    /// <param name="forcePartialRebuild">Force la réindexation partielle, même si l'index n'existait pas avant.</param>
    /// <returns>Le nombre de documents.</returns>
    int RebuildIndex<TDocument>(ILogger? rebuildLogger = null, bool forcePartialRebuild = false)
        where TDocument : class;
}
