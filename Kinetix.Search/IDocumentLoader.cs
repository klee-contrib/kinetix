using System.Collections.Generic;

namespace Kinetix.Search
{
    /// <summary>
    /// Contrat pour les loaders de documents pour indexation.
    /// </summary>
    /// <typeparam name="TDocument">Type de document.</typeparam>
    public interface IDocumentLoader<TDocument>
    {
        /// <summary>
        /// Charge un document pour indexation
        /// </summary>
        /// <param name="id">Id du document.</param>
        /// <returns>Le document.</returns>
        TDocument Get(int id);

        /// <summary>
        /// Charge tous les documents pour indexation.
        /// </summary>
        /// <returns>Les documents.</returns>
        IEnumerable<TDocument> GetAll();

        /// <summary>
        /// Charge plusieurs documents pour indexation.
        /// </summary>
        /// <param name="ids">Ids des documents.</param>
        /// <returns>Les documents.</returns>
        IEnumerable<TDocument> GetMany(IEnumerable<int> ids);
    }
}
