using System;
using System.Collections.Generic;
using Kinetix.Search.ComponentModel;
using Kinetix.Search.Model;

namespace Kinetix.Search
{
    /// <summary>
    /// Contrat des stores de recherche.
    /// </summary>
    /// <typeparam name="TDocument">Type du document.</typeparam>
    public interface ISearchStore<TDocument>
        where TDocument : class
    {
        /// <summary>
        /// Créé l'index.
        /// </summary>
        void CreateDocumentType();

        /// <summary>
        /// Obtient un document à partir de son ID.
        /// </summary>
        /// <param name="id">ID du document.</param>
        /// <returns>Document.</returns>
        TDocument Get(string id);

        /// <summary>
        /// Obtient un document à partir de sa clé primaire composite.
        /// </summary>
        /// <param name="bean">Le bean, avec sa clé primaire composite renseignée.</param>
        /// <returns>Document.</returns>
        TDocument Get(TDocument bean);

        /// <summary>
        /// Pose un document dans l'index.
        /// </summary>
        /// <param name="document">Document à poser.</param>
        void Put(TDocument document);

        /// <summary>
        /// Pose un ensemble de documents dans l'index.
        /// </summary>
        /// <param name="documentList">Liste de documents.</param>
        void PutAll(IEnumerable<TDocument> documentList);

        /// <summary>
        /// Supprime un document dans l'index.
        /// </summary>
        /// <param name="id">ID du document.</param>
        void Remove(string id);

        /// <summary>
        /// Supprime un document dans l'index.
        /// </summary>
        /// <param name="bean">Le document.</param>
        void Remove(TDocument bean);

        /// <summary>
        /// Supprime tous les documents.
        /// </summary>
        void Flush();

        /// <summary>
        /// Effectue une recherche avancé.
        /// </summary>
        /// <param name="input">Entrée de la recherche.</param>
        /// <returns>Sortie de la recherche.</returns>
        QueryOutput<TDocument> AdvancedQuery<TCriteria>(AdvancedQueryInput<TDocument, TCriteria> input)
            where TCriteria : Criteria;

        /// <summary>
        /// Effectue une recherche avancé.
        /// </summary>
        /// <param name="input">Entrée de la recherche.</param>
        /// <param name="documentMapper">Mapper pour convertir le document dans le bon type de sortie.</param>
        /// <returns>Sortie de la recherche.</returns>
        QueryOutput<TOutput> AdvancedQuery<TOutput, TCriteria>(AdvancedQueryInput<TDocument, TCriteria> input, Func<TDocument, TOutput> documentMapper)
            where TCriteria : Criteria;

        /// <summary>
        /// Effectue un count avancé.
        /// </summary>
        /// <param name="input">Entrée de la recherche.</param>
        /// <returns>Nombre de documents.</returns>
        long AdvancedCount<TCriteria>(AdvancedQueryInput<TDocument, TCriteria> input)
            where TCriteria : Criteria;

        /// <summary>
        /// Enregistre une data source.
        /// </summary>
        /// <param name="dataSourceName">Nom de la data source.</param>
        /// <returns>This.</returns>
        ISearchStore<TDocument> RegisterDataSource(string dataSourceName);
    }
}
