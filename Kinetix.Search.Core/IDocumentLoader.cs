using System.Reflection;
using Kinetix.Search.Models.Annotations;

namespace Kinetix.Search.Core;

/// <summary>
/// Contrat pour les loaders de documents pour indexation.
/// </summary>
/// <typeparam name="TDocument">Type de document.</typeparam>
/// <typeparam name="TKey">Type de clé primaire</typeparam>
public interface IDocumentLoader<TDocument, TKey>
    where TDocument : class, new()
{
    /// <summary>
    /// Renseigne un document ES avec les champs qui constituent son Id.
    /// </summary>
    /// <param name="id">Id du document.</param>
    /// <returns>Document ES avec les champs d'id renseignés.</returns>
    TDocument FillDocumentWithKey(TKey id)
    {
        var doc = new TDocument();

        var idProperties = typeof(TDocument).GetProperties().Where(p => p.GetCustomAttribute<SearchFieldAttribute>()?.Category == SearchFieldCategory.Id);
        if (idProperties.Count() != 1 || !idProperties.Single().PropertyType.IsAssignableFrom(typeof(TKey)))
        {
            throw new NotImplementedException("Impossible de récupérer l'id du document automatiquement. Veuillez implémenter `FillDocumentWithKey`.");
        }

        idProperties.Single().SetValue(doc, id);
        return doc;
    }

    /// <summary>
    /// Charge un document pour indexation.
    /// </summary>
    /// <param name="id">Id du document.</param>
    /// <returns>Le document.</returns>
    TDocument Get(TKey id);

    /// <summary>
    /// Charge tous les documents pour indexation.
    /// </summary>
    /// <param name="partialRebuild">Indique que l'on veut un rebuild partiel, donc certains documents peuvent être ignorés.</param>
    /// <returns>Les documents.</returns>
    IEnumerable<TDocument> GetAll(bool partialRebuild);

    /// <summary>
    /// Charge plusieurs documents pour indexation.
    /// </summary>
    /// <param name="ids">Ids des documents.</param>
    /// <returns>Les documents.</returns>
    IEnumerable<TDocument> GetMany(IEnumerable<TKey> ids);
}
