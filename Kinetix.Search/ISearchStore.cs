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
        /// Pose un document dans l'index.
        /// </summary>
        /// <param name="document">Document à poser.</param>
        void Put<TDocument>(TDocument document)
            where TDocument : class;

        /// <summary>
        /// Pose un ensemble de documents dans l'index.
        /// </summary>
        /// <param name="documentList">Liste de documents.</param>
        /// <param name="waitForRefresh">Attends le refresh de l'index avant de répondre.</param>
        void PutAll<TDocument>(IEnumerable<TDocument> documentList, bool waitForRefresh = false)
            where TDocument : class;

        /// <summary>
        /// Supprime un document dans l'index.
        /// </summary>
        /// <param name="id">ID du document.</param>
        void Remove(string id);

        /// <summary>
        /// Supprime un document dans l'index.
        /// </summary>
        /// <param name="bean">Le document.</param>
        void Remove<TDocument>(TDocument bean)
            where TDocument : class;

        /// <summary>
        /// Supprime tous les documents.
        /// </summary>
        void Flush<TDocument>()
            where TDocument : class;

        /// <summary>
        /// Effectue une recherche avancée.
        /// </summary>
        /// <param name="input">Entrée de la recherche.</param>
        /// <param name="documentMapper">Mapper pour convertir le document dans le bon type de sortie.</param>
        /// <returns>Sortie de la recherche.</returns>
        QueryOutput<TOutput> AdvancedQuery<TDocument, TOutput, TCriteria>(AdvancedQueryInput<TDocument, TCriteria> input, Func<TDocument, TOutput> documentMapper)
            where TDocument : class
            where TCriteria : Criteria;

        /// <summary>
        /// Effectue un count avancé.
        /// </summary>
        /// <param name="input">Entrée de la recherche.</param>
        /// <returns>Nombre de documents.</returns>
        long AdvancedCount<TDocument, TCriteria>(AdvancedQueryInput<TDocument, TCriteria> input)
            where TDocument : class
            where TCriteria : Criteria;
    }
}
