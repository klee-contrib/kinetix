namespace Kinetix.Search;

internal interface IIndexingDocumentState
{
}

/// <summary>
/// Contient l'état de réindexation en cours d'un document.
/// </summary>
/// <typeparam name="TDocument">Type de document.</typeparam>
internal class IndexingDocumentState<TDocument> : IIndexingDocumentState
{
    public HashSet<TDocument> BeansToDelete { get; } = new();
    public HashSet<TDocument> BeansToIndex { get; } = new();
    public HashSet<int> IdsToDelete { get; } = new();
    public HashSet<int> IdsToIndex { get; } = new();
    public bool Reindex { get; set; } = false;

    /// <summary>
    /// Marque un document pour suppression dans son index.
    /// </summary>
    /// <param name="id">ID du document.</param>
    /// <returns>Succès.</returns>
    public bool RegisterDelete(int id)
    {
        IdsToIndex.Remove(id);
        return IdsToDelete.Add(id);
    }

    /// <summary>
    /// Marque un document pour suppression dans son index.
    /// </summary>
    /// <param name="bean">La clé composite.</param>
    /// <returns>Succès.</returns>
    public bool RegisterDelete(TDocument bean)
    {
        BeansToIndex.Remove(bean);
        return BeansToDelete.Add(bean);
    }

    /// <summary>
    /// Marque un document pour (ré)indexation.
    /// </summary>
    /// <param name="id">ID du document.</param>
    /// <returns>Succès.</returns>
    public bool RegisterIndex(int id)
    {
        return !IdsToDelete.Contains(id) && IdsToIndex.Add(id);
    }

    /// <summary>
    /// Marque un document pour (ré)indexation.
    /// </summary>
    /// <param name="bean">La clé composite.</param>
    /// <returns>Succès.</returns>
    public bool RegisterIndex(TDocument bean)
    {
        return !BeansToDelete.Contains(bean) && BeansToIndex.Add(bean);
    }
}
