namespace Kinetix.Search;

/// <summary>
/// Contrat pour les loaders de documents pour indexation.
/// </summary>
/// <typeparam name="TDocument">Type de document.</typeparam>
public interface IDocumentLoader<TDocument>
{
    /// <summary>
    /// Charge un document pour indexation par id.
    /// </summary>
    /// <param name="id">Id du document.</param>
    /// <returns>Le document.</returns>
    TDocument Get(int id);

    /// <summary>
    /// Charge un document pour indexation par clé composite.
    /// </summary>
    /// <param name="bean">Clé composite.</param>
    /// <returns>Le document.</returns>
    TDocument Get(TDocument bean);

    /// <summary>
    /// Charge tous les documents pour indexation.
    /// </summary>
    /// <param name="partialRebuild">Indique que l'on veut un rebuild partiel, donc certains documents peuvent être ignorés.</param>
    /// <returns>Les documents.</returns>
    IEnumerable<TDocument> GetAll(bool partialRebuild);

    /// <summary>
    /// Charge plusieurs documents par id pour indexation.
    /// </summary>
    /// <param name="ids">Ids des documents.</param>
    /// <returns>Les documents.</returns>
    IEnumerable<TDocument> GetMany(IEnumerable<int> ids);

    /// <summary>
    /// Charge plusieurs documents par clé composite pour indexation.
    /// </summary>
    /// <param name="beans">Clés composites.</param>
    /// <returns>Les documents.</returns>
    IEnumerable<TDocument> GetMany(IEnumerable<TDocument> beans);
}
