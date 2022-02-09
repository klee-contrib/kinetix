using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Kinetix.Search
{
    /// <summary>
    /// Interface non générique pour DocumentIndexor<TDocument>.
    /// </summary>
    interface IDocumentIndexor
    {
        /// <summary>
        /// Charge les documents à indexer et supprimer dans le SearchBulkDescriptor fourni.
        /// </summary>
        /// <param name="bulk">SearchBulkDescriptor.</param>
        /// <returns>SearchBulkDescriptor.</returns>
        ISearchBulkDescriptor PrepareBulkDescriptor(ISearchBulkDescriptor bulk);
    }

    /// <summary>
    /// Indexeur de document, s'occupe du stockage des clés de documents et de leur récupération, pour préparation de la requête dans le store.
    /// </summary>
    /// <typeparam name="TDocument">Type de document.</typeparam>
    class DocumentIndexor<TDocument> : IDocumentIndexor
        where TDocument : class
    {
        private readonly IServiceProvider _provider;

        private readonly HashSet<TDocument> _beansToDelete = new HashSet<TDocument>();
        private readonly HashSet<TDocument> _beansToIndex = new HashSet<TDocument>();
        private readonly HashSet<int> _idsToDelete = new HashSet<int>();
        private readonly HashSet<int> _idsToIndex = new HashSet<int>();

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="provider">ServiceProvider.</param>
        public DocumentIndexor(IServiceProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// Réconstruit tout l'index. A la priorité sur toutes les autres demandes.
        /// </summary>
        public bool Reindex { get; set; } = false;

        /// <summary>
        /// Charge tous les documents (pour reconstruction de l'index).
        /// </summary>
        /// <param name="partialRebuild">Reconstruction partielle (si un index à jour existe déjà).</param>
        /// <returns>Tous les documents.</returns>
        public IEnumerable<TDocument> LoadAllDocuments(bool partialRebuild)
        {
            return _provider.GetService<IDocumentLoader<TDocument>>().GetAll(partialRebuild);
        }

        /// <inheritDoc cref="IDocumentIndexor.PrepareBulkDescriptor" />
        public ISearchBulkDescriptor PrepareBulkDescriptor(ISearchBulkDescriptor bulk)
        {
            var loader = _provider.GetService<IDocumentLoader<TDocument>>();
            if (Reindex)
            {
                var docs = loader.GetAll(false);
                return docs.Any()
                    ? bulk.IndexMany(docs)
                    : bulk;
            }
            else
            {
                if (_idsToDelete.Count == 1)
                {
                    bulk.Delete<TDocument>(_idsToDelete.Single().ToString());
                }
                else if (_idsToDelete.Count > 1)
                {
                    bulk.DeleteMany<TDocument>(_idsToDelete.Select(id => id.ToString()));
                }

                if (_beansToDelete.Count == 1)
                {
                    bulk.Delete(_beansToDelete.Single());
                }
                else if (_beansToDelete.Count > 1)
                {
                    bulk.DeleteMany(_beansToDelete.ToList());
                }

                if (_idsToIndex.Count == 1)
                {
                    var doc = loader.Get(_idsToIndex.Single());
                    if (doc != null)
                    {
                        bulk.Index(doc);
                    }
                }
                else if (_idsToIndex.Count > 1)
                {
                    var docs = loader.GetMany(_idsToIndex.ToList());
                    if (docs.Any())
                    {
                        bulk.IndexMany(docs);
                    }
                }

                if (_beansToIndex.Count == 1)
                {
                    var doc = loader.Get(_beansToIndex.Single());
                    if (doc != null)
                    {
                        bulk.Index(doc);
                    }
                }
                else if (_beansToIndex.Count > 1)
                {
                    var docs = loader.GetMany(_beansToIndex.ToList());
                    if (docs.Any())
                    {
                        bulk.IndexMany(docs);
                    }
                }

                return bulk;
            }
        }

        /// <summary>
        /// Marque un document pour suppression dans son index.
        /// </summary>
        /// <param name="id">ID du document.</param>
        /// <returns>Succès.</returns>
        public bool RegisterDelete(int id)
        {
            _idsToIndex.Remove(id);
            return _idsToDelete.Add(id);
        }

        /// <summary>
        /// Marque un document pour suppression dans son index.
        /// </summary>
        /// <param name="bean">La clé composite.</param>
        /// <returns>Succès.</returns>
        public bool RegisterDelete(TDocument bean)
        {
            _beansToIndex.Remove(bean);
            return _beansToDelete.Add(bean);
        }

        /// <summary>
        /// Marque un document pour (ré)indexation.
        /// </summary>
        /// <param name="id">ID du document.</param>
        /// <returns>Succès.</returns>
        public bool RegisterIndex(int id)
        {
            return !_idsToDelete.Contains(id) && _idsToIndex.Add(id);
        }

        /// <summary>
        /// Marque un document pour (ré)indexation.
        /// </summary>
        /// <param name="bean">La clé composite.</param>
        /// <returns>Succès.</returns>
        public bool RegisterIndex(TDocument bean)
        {
            return !_beansToDelete.Contains(bean) && _beansToIndex.Add(bean);
        }
    }
}
