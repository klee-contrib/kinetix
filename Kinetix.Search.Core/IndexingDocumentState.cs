namespace Kinetix.Search.Core;

internal interface IIndexingDocumentState
{
}

/// <summary>
/// Contient l'état de réindexation en cours d'un document.
/// </summary>
/// <typeparam name="TDocument">Type de document.</typeparam>
/// <typeparam name="TKey">Type de clé primaire.</typeparam>
internal class IndexingDocumentState<TDocument, TKey> : IIndexingDocumentState
    where TDocument : class
{
    public HashSet<TKey> IdsToDelete { get; } = new();
    public HashSet<TKey> IdsToIndex { get; } = new();
    public bool Reindex { get; set; } = false;

    /// <summary>
    /// Marque un document pour suppression dans son index.
    /// </summary>
    /// <param name="id">ID du document.</param>
    /// <returns>Succès.</returns>
    public bool RegisterDelete(TKey id)
    {
        IdsToIndex.Remove(id);
        return IdsToDelete.Add(id);
    }

    /// <summary>
    /// Marque un document pour (ré)indexation.
    /// </summary>
    /// <param name="id">ID du document.</param>
    /// <returns>Succès.</returns>
    public bool RegisterIndex(TKey id)
    {
        return !IdsToDelete.Contains(id) && IdsToIndex.Add(id);
    }
}
