using System;
using System.Collections.Generic;
using Kinetix.Search.ComponentModel;
using Kinetix.Search.Model;

namespace Kinetix.Search
{
    /// <summary>
    /// Contrat des stores de recherche.
    /// </summary>
    public interface ISearchStore
    {
        /// <summary>
        /// Créé l'index.
        /// </summary>
        void CreateDocumentType<TDocument>()
            where TDocument : class;

        /// <summary>
        /// Obtient un document à partir de son ID.
        /// </summary>
        /// <param name="id">ID du document.</param>
        /// <returns>Document.</returns>
        TDocument Get<TDocument>(string id)
            where TDocument : class;

        /// <summary>
        /// Obtient un document à partir de sa clé primaire composite.
        /// </summary>
        /// <param name="bean">Le bean, avec sa clé primaire composite renseignée.</param>
        /// <returns>Document.</returns>
        TDocument Get<TDocument>(TDocument bean)
            where TDocument : class;

        /// <summary>
        /// Permet d'effectuer des indexations et de suppressions en masse.
        /// </summary>
        /// <returns>ISearchBulkDescriptor.</returns>
        ISearchBulkDescriptor Bulk();

        /// <summary>
        /// Supprime un document de l'index.
        /// </summary>
        /// <param name="id">ID du document.</param>
        void Delete(string id);

        /// <summary>
        /// Supprime un document de l'index.
        /// </summary>
        /// <param name="bean">La clé composite.</param>
        void Delete<TDocument>(TDocument bean)
            where TDocument : class;

        /// <summary>
        /// Pose un document dans l'index.
        /// </summary>
        /// <param name="document">Document à poser.</param>
        void Index<TDocument>(TDocument document)
            where TDocument : class;

        /// <summary>
        /// Réinitialise l'index avec les documents fournis.
        /// </summary>
        /// <param name="documentList">Liste de documents.</param>
        void IndexAll<TDocument>(IEnumerable<TDocument> documentList)
            where TDocument : class;

        /// <summary>
        /// Effectue une recherche avancée.
        /// </summary>
        /// <param name="input">Entrée de la recherche.</param>
        /// <param name="documentMapper">Mapper pour convertir le document dans le bon type de sortie.</param>
        /// <returns>Sortie de la recherche.</returns>
        QueryOutput<TOutput> AdvancedQuery<TDocument, TOutput, TCriteria>(AdvancedQueryInput<TDocument, TCriteria> input, Func<TDocument, TOutput> documentMapper)
            where TDocument : class
            where TCriteria : Criteria, new();

        /// <summary>
        /// Effectue une recherche avancée mutiple.
        /// </summary>
        /// <returns>Descripteur.</returns>
        IMultiAdvancedQueryDescriptor MultiAdvancedQuery();

        /// <summary>
        /// Effectue un count avancé.
        /// </summary>
        /// <param name="input">Entrée de la recherche.</param>
        /// <returns>Nombre de documents.</returns>
        long AdvancedCount<TDocument, TCriteria>(AdvancedQueryInput<TDocument, TCriteria> input)
            where TDocument : class
            where TCriteria : Criteria, new();
    }
}
