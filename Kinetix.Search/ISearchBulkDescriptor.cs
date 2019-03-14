using System.Collections.Generic;

namespace Kinetix.Search
{
    /// <summary>
    /// Permet de réaliser des indexations et suppressions en masse.
    /// </summary>
    public interface ISearchBulkDescriptor
    {
        /// <summary>
        /// Supprime un document de l'index.
        /// </summary>
        /// <typeparam name="TDocument">Type de document.</typeparam>
        /// <param name="id">ID du document.</param>
        /// <returns>ISearchBulkDescriptor.</returns>
        ISearchBulkDescriptor Delete<TDocument>(string id)
            where TDocument : class;

        /// <summary>
        /// Supprime un document de l'index.
        /// </summary>
        /// <typeparam name="TDocument">Type de document.</typeparam>
        /// <param name="bean">La clé composite.</param>
        /// <returns>ISearchBulkDescriptor.</returns>
        ISearchBulkDescriptor Delete<TDocument>(TDocument bean)
            where TDocument : class;

        /// <summary>
        /// Supprime des documents de l'index.
        /// </summary>
        /// <typeparam name="TDocument">Type de document.</typeparam>
        /// <param name="ids">IDs des document.</param>
        /// <returns>ISearchBulkDescriptor.</returns>
        ISearchBulkDescriptor DeleteMany<TDocument>(IEnumerable<string> ids)
            where TDocument : class;

        /// <summary>
        /// Supprime des document de l'index.
        /// </summary>
        /// <typeparam name="TDocument">Type de document.</typeparam>
        /// <param name="beans">Les clés composites.</param>
        /// <returns>ISearchBulkDescriptor.</returns>
        ISearchBulkDescriptor DeleteMany<TDocument>(IEnumerable<TDocument> beans)
            where TDocument : class;

        /// <summary>
        /// Pose un document dans l'index.
        /// </summary>
        /// <typeparam name="TDocument">Type de document.</typeparam>
        /// <param name="document">Document à poser.</param>
        /// <returns>ISearchBulkDescriptor.</returns>
        ISearchBulkDescriptor Index<TDocument>(TDocument document)
            where TDocument : class;

        /// <summary>
        /// Pose des documents dans l'index.
        /// </summary>
        /// <typeparam name="TDocument">Type de document.</typeparam>
        /// <param name="documents">Documents à poser.</param>
        /// <returns>ISearchBulkDescriptor.</returns>
        ISearchBulkDescriptor IndexMany<TDocument>(IEnumerable<TDocument> documents)
            where TDocument : class;

        /// <summary>
        /// Effectue la requête.
        /// </summary>
        /// <param name="refresh">Attends ou non la réindexation.</param>
        void Run(bool refresh = true);
    }
}