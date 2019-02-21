using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Kinetix.Search
{
    /// <summary>
    /// Gère la réindexation des documents.
    /// </summary>
    public class IndexManager
    {
        private readonly IServiceProvider _provider;
        private readonly ISearchStore _searchStore;

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="provider">Service provider.</param>
        /// <param name="searchStore">Composant injecté.</param>
        public IndexManager(IServiceProvider provider, ISearchStore searchStore)
        {
            _provider = provider;
            _searchStore = searchStore;
        }

        /// <summary>
        /// Permet d'effectuer des indexations et de suppressions en masse.
        /// </summary>
        /// <returns>BulkIndexDescriptor.</returns>
        public BulkIndexDescriptor Bulk()
        {
            return new BulkIndexDescriptor(_provider, _searchStore);
        }

        /// <summary>
        /// Supprime un document de l'index.
        /// </summary>
        /// <param name="id">ID du document.</param>
        public void Delete<TDocument>(int id)
            where TDocument : class
        {
            _searchStore.Delete<TDocument>(id.ToString());
        }

        /// <summary>
        /// Supprime un document de l'index.
        /// </summary>
        /// <param name="bean">La clé composite.</param>
        public void Delete<TDocument>(TDocument bean)
            where TDocument : class
        {
            _searchStore.Delete(bean);
        }

        /// <summary>
        /// Pose un document dans l'index.
        /// </summary>
        /// <param name="id">Id du document à poser.</param>
        public void Index<TDocument>(int id)
            where TDocument : class
        {
            var doc = _provider.GetService<IDocumentLoader<TDocument>>().Get(id);
            if (doc != null)
            {
                _searchStore.Index(doc);
            }
        }

        /// <summary>
        /// Réinitialise l'index.
        /// </summary>
        /// <returns>Le nombre d'éléments indexés.</returns>
        public int IndexAll<TDocument>()
           where TDocument : class
        {
            var docs = _provider.GetService<IDocumentLoader<TDocument>>().GetAll();
            if (docs.Any())
            {
                _searchStore.IndexAll(docs);
            }

            return docs.Count();
        }
    }
}
