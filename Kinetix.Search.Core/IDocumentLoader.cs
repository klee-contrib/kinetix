#pragma warning disable MA0048

namespace Kinetix.Search.Core;

/// <summary>
/// Contrat pour les loaders de documents pour indexation.
/// </summary>
/// <typeparam name="TDocument">Type de document.</typeparam>
public interface IDocumentLoader<TDocument>
    where TDocument : class
{
    /// <summary>
    /// Charge tous les documents pour indexation.
    /// </summary>
    /// <param name="partialRebuild">Indique que l'on veut un rebuild partiel, donc certains documents peuvent être ignorés.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Les documents.</returns>
    IAsyncEnumerable<TDocument> GetAllAsync(bool partialRebuild, CancellationToken ct = default);

    /// <summary>
    /// Charge un document pour indexation.
    /// </summary>
    /// <param name="id">Id du document.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Le document.</returns>
    Task<TDocument> GetAsync(object id, CancellationToken ct = default);

    /// <summary>
    /// Charge plusieurs documents pour indexation.
    /// </summary>
    /// <param name="ids">Ids des documents.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Les documents.</returns>
    IAsyncEnumerable<TDocument> GetManyAsync(IEnumerable<object> ids, CancellationToken ct = default);
}

/// <summary>
/// Implé abstraite de IDocumentLoader avec la clé primaire typée.
/// </summary>
/// <typeparam name="TDocument">Type de document.</typeparam>
/// <typeparam name="TKey">Type de la clé primaire. Si la clé est composite, alors le type doit être un tuple avec les propriétés dans le bon ordre.</typeparam>
public abstract class DocumentLoader<TDocument, TKey> : IDocumentLoader<TDocument>
    where TDocument : class
{
    /// <inheritdoc cref="IDocumentLoader{TDocument}.GetAllAsync" />
    public abstract IAsyncEnumerable<TDocument> GetAllAsync(bool partialRebuild, CancellationToken ct = default);

    /// <inheritdoc cref="IDocumentLoader{TDocument}.GetAsync" />
    public Task<TDocument> GetAsync(object id, CancellationToken ct = default)
    {
        return GetAsync((TKey)id, ct);
    }

    /// <summary>
    /// Charge un document pour indexation.
    /// </summary>
    /// <param name="id">Id du document.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Le document.</returns>
    public abstract Task<TDocument> GetAsync(TKey id, CancellationToken ct = default);

    /// <summary>
    /// Charge plusieurs documents pour indexation.
    /// </summary>
    /// <param name="ids">Ids des documents.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Les documents.</returns>
    public abstract IAsyncEnumerable<TDocument> GetMany(IEnumerable<TKey> ids, CancellationToken ct = default);

    /// <inheritdoc cref="IDocumentLoader{TDocument}.GetManyAsync" />
    public IAsyncEnumerable<TDocument> GetManyAsync(IEnumerable<object> ids, CancellationToken ct = default)
    {
        return GetMany(ids.Cast<TKey>(), ct);
    }
}
