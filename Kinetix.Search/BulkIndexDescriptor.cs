using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Kinetix.Search
{
    /// <summary>
    /// Descripteur pour les réindexation en masse via l'index manager.
    /// </summary>
    public class BulkIndexDescriptor
    {
        private readonly IServiceProvider _provider;
        private readonly ISearchBulkDescriptor _searchBulkDescriptor;

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="provider">Composant injecté.</param>
        /// <param name="searchStore">Composant injecté.</param>
        internal BulkIndexDescriptor(IServiceProvider provider, ISearchStore searchStore)
        {
            _provider = provider;
            _searchBulkDescriptor = searchStore.Bulk();
        }

        /// <summary>
        /// Supprime un document de l'index.
        /// </summary>
        /// <typeparam name="TDocument">Type de document.</typeparam>
        /// <param name="id">ID du document.</param>
        /// <returns>ISearchBulkDescriptor.</returns>
        public BulkIndexDescriptor Delete<TDocument>(int id)
            where TDocument : class
        {
            _searchBulkDescriptor.Delete<TDocument>(id.ToString());
            return this;
        }

        /// <summary>
        /// Supprime un document de l'index.
        /// </summary>
        /// <typeparam name="TDocument">Type de document.</typeparam>
        /// <param name="bean">La clé composite.</param>
        /// <returns>ISearchBulkDescriptor.</returns>
        public BulkIndexDescriptor Delete<TDocument>(TDocument bean)
           where TDocument : class
        {
            _searchBulkDescriptor.Delete(bean);
            return this;
        }

        /// <summary>
        /// Supprime des documents de l'index.
        /// </summary>
        /// <typeparam name="TDocument">Type de document.</typeparam>
        /// <param name="ids">IDs des document.</param>
        /// <returns>ISearchBulkDescriptor.</returns>
        public BulkIndexDescriptor DeleteMany<TDocument>(IEnumerable<int> ids)
            where TDocument : class
        {
            _searchBulkDescriptor.DeleteMany(ids.Select(id => id.ToString()));
            return this;
        }

        /// <summary>
        /// Supprime des document de l'index.
        /// </summary>
        /// <typeparam name="TDocument">Type de document.</typeparam>
        /// <param name="beans">Les clés composites.</param>
        /// <returns>ISearchBulkDescriptor.</returns>
        public BulkIndexDescriptor DeleteMany<TDocument>(IEnumerable<TDocument> beans)
           where TDocument : class
        {
            _searchBulkDescriptor.DeleteMany(beans);
            return this;
        }

        /// <summary>
        /// Pose un document dans l'index.
        /// </summary>
        /// <typeparam name="TDocument">Type de document.</typeparam>
        /// <param name="id">Id du document à poser.</param>
        /// <returns>ISearchBulkDescriptor.</returns>
        public BulkIndexDescriptor Index<TDocument>(int id)
            where TDocument : class
        {
            var doc = _provider.GetService<IDocumentLoader<TDocument>>().Get(id);
            if (doc != null)
            {
                _searchBulkDescriptor.Index(doc);
            }

            return this;
        }

        /// <summary>
        /// Pose des documents dans l'index.
        /// </summary>
        /// <typeparam name="TDocument">Type de document.</typeparam>
        /// <param name="ids">Ids des documents à poser.</param>
        /// <returns>ISearchBulkDescriptor.</returns>
        public BulkIndexDescriptor IndexMany<TDocument>(IEnumerable<int> ids)
            where TDocument : class
        {
            var docs = _provider.GetService<IDocumentLoader<TDocument>>().GetMany(ids);
            if (docs.Any())
            {
                _searchBulkDescriptor.IndexMany(docs);
            }

            return this;
        }

        /// <summary>
        /// Effectue la requête.
        /// </summary>
        /// <param name="refresh">Attends ou non la réindexation.</param>
        public void Run(bool refresh = true)
        {
            _searchBulkDescriptor.Run(refresh);
        }
    }
}
