using System;
using System.Collections.Generic;
using System.Linq;
using Kinetix.Search.Config;
using Microsoft.Extensions.Logging;

namespace Kinetix.Search
{
    /// <summary>
    /// Gère la réindexation des documents.
    /// </summary>
    public class IndexManager
    {
        private readonly ILogger<IndexManager> _logger;
        private readonly IServiceProvider _provider;
        private readonly ISearchStore _searchStore;

        private readonly Dictionary<Type, IDocumentIndexor> _indexors = new Dictionary<Type, IDocumentIndexor>();

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="provider">Composant injecté.</param>
        /// <param name="searchStore">Composant injecté.</param>
        public IndexManager(ILogger<IndexManager> logger, IServiceProvider provider, ISearchStore searchStore)
        {
            _logger = logger;
            _provider = provider;
            _searchStore = searchStore;
        }

        /// <summary>
        /// Attends ou non la réindexation dans ES avant de continuer (Refresh.WaitFor).
        /// </summary>
        public bool Refresh { get; set; } = true;

        /// <summary>
        /// Marque un document pour suppression dans son index.
        /// </summary>
        /// <typeparam name="TDocument">Type de document.</typeparam>
        /// <param name="id">ID du document.</param>
        /// <returns>IndexManager.</returns>
        public IndexManager Delete<TDocument>(int id)
            where TDocument : class
        {
            _logger.LogInformation($"RegisterDelete 1 {typeof(TDocument).Name}");
            GetIndexor<TDocument>().RegisterDelete(id);
            return this;
        }

        /// <summary>
        /// Marque un document pour suppression dans son index.
        /// </summary>
        /// <typeparam name="TDocument">Type de document.</typeparam>
        /// <param name="bean">La clé composite.</param>
        /// <returns>IndexManager.</returns>
        public IndexManager Delete<TDocument>(TDocument bean)
            where TDocument : class
        {
            _logger.LogInformation($"RegisterDelete 1 {typeof(TDocument).Name}");
            GetIndexor<TDocument>().RegisterDelete(bean);
            return this;
        }

        /// <summary>
        /// Marque plusieurs documents pour suppression dans leur index.
        /// </summary>
        /// <typeparam name="TDocument">Type de document.</typeparam>
        /// <param name="ids">IDs des documents.</param>
        /// <returns>IndexManager.</returns>
        public IndexManager DeleteMany<TDocument>(IEnumerable<int> ids)
            where TDocument : class
        {
            _logger.LogInformation($"RegisterDelete {ids.Count()} {typeof(TDocument).Name}");
            foreach (var id in ids)
            {
                Delete<TDocument>(id);
            }

            return this;
        }

        /// <summary>
        /// Marque plusieurs documents pour suppression dans leur index.
        /// </summary>
        /// <typeparam name="TDocument">Type de document.</typeparam>
        /// <param name="beans">Clé composites des documents.</param>
        /// <returns>IndexManager.</returns>
        public IndexManager DeleteMany<TDocument>(IEnumerable<TDocument> beans)
           where TDocument : class
        {
            _logger.LogInformation($"RegisterDelete {beans.Count()} {typeof(TDocument).Name}");
            foreach (var bean in beans)
            {
                Delete(bean);
            }

            return this;
        }

        /// <summary>
        /// Marque un document pour (ré)indexation.
        /// </summary>
        /// <typeparam name="TDocument">Type de document.</typeparam>
        /// <param name="id">ID du document.</param>
        /// <returns>IndexManager.</returns>
        public IndexManager Index<TDocument>(int id)
            where TDocument : class
        {
            _logger.LogInformation($"RegisterIndex 1 {typeof(TDocument).Name}");
            GetIndexor<TDocument>().RegisterIndex(id);
            return this;
        }

        /// <summary>
        /// Marque un document pour (ré)indexation.
        /// </summary>
        /// <typeparam name="TDocument">Type de document.</typeparam>
        /// <param name="bean">La clé composite.</param>
        /// <returns>IndexManager.</returns>
        public IndexManager Index<TDocument>(TDocument bean)
            where TDocument : class
        {
            _logger.LogInformation($"RegisterIndex 1 {typeof(TDocument).Name}");
            GetIndexor<TDocument>().RegisterIndex(bean);
            return this;
        }

        /// <summary>
        /// Marque plusieurs documents pour ré(indexation).
        /// </summary>
        /// <typeparam name="TDocument">Type de document.</typeparam>
        /// <param name="ids">IDs des documents.</param>
        /// <returns>IndexManager.</returns>
        public IndexManager IndexMany<TDocument>(IEnumerable<int> ids)
            where TDocument : class
        {
            _logger.LogInformation($"RegisterIndex {ids.Count()} {typeof(TDocument).Name}");
            foreach (var id in ids)
            {
                Index<TDocument>(id);
            }

            return this;
        }

        /// <summary>
        /// Marque plusieurs documents pour ré(indexation).
        /// </summary>
        /// <typeparam name="TDocument">Type de document.</typeparam>
        /// <param name="beans">Clé composites des documents.</param>
        /// <returns>IndexManager.</returns>
        public IndexManager IndexMany<TDocument>(IEnumerable<TDocument> beans)
           where TDocument : class
        {
            _logger.LogInformation($"RegisterIndex {beans.Count()} {typeof(TDocument).Name}");
            foreach (var bean in beans)
            {
                Index(bean);
            }

            return this;
        }

        /// <summary>
        /// Réinitialise un index.
        /// </summary>
        /// <typeparam name="TDocument">Type de document.</typeparam>
        public IndexManager IndexAll<TDocument>()
           where TDocument : class
        {
            _logger.LogInformation($"Reindex {typeof(TDocument).Name}");
            GetIndexor<TDocument>().Reindex = true;
            return this;
        }

        /// <summary>
        /// Lance la suppression et la réindexation de tous les documents enregistrés dans cette instance de l'IndexManager.
        /// A priori, cette méthode est lancée automatiquement à la fin de la transaction en cours, donc il ne devrait pas y avoir besoin de la lancer manuellement.
        /// </summary>
        /// <returns></returns>
        public int Flush()
        {
            var bulk = _searchStore.Bulk();

            try
            {
                foreach (var indexor in _indexors)
                {
                    _logger.LogInformation($"Prepare {indexor.Key.Name}");
                    indexor.Value.PrepareBulkDescriptor(bulk);
                }

                return bulk.Run(Refresh);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while flushing : ");
                throw;
            }
            finally
            {
                _indexors.Clear();
            }
        }

        /// <summary>
        /// Reconstruit un index.
        /// </summary>
        /// <typeparam name="TDocument">Type de document.</typeparam>
        /// <param name="rebuildLogger">Logger custom pour suivre l'avancement de la réindexation.</param>
        /// <returns>Le nombre de documents.</returns>
        public int RebuildIndex<TDocument>(ILogger rebuildLogger = null)
            where TDocument : class
        {
            var indexName = SearchConfig.GetTypeNameForIndex(typeof(TDocument));

            rebuildLogger?.LogInformation($"Index {indexName} rebuild started...");
            var indexCreated = _searchStore.EnsureIndex<TDocument>();
            if (indexCreated)
            {
                rebuildLogger?.LogInformation($"Index {indexName} (re)created.");
            }

            rebuildLogger?.LogInformation($"Loading data for index {indexName}...");
            var documents = GetIndexor<TDocument>().LoadAllDocuments(!indexCreated);
            rebuildLogger?.LogInformation($"Data for index {indexName} loaded.");
            if (documents is ICollection<TDocument> coll)
            {
                rebuildLogger?.LogInformation($"{coll.Count} documents ready for indexation.");
            }

            return _searchStore.ResetIndex(documents, !indexCreated, rebuildLogger);
        }

        private DocumentIndexor<TDocument> GetIndexor<TDocument>()
            where TDocument : class
        {
            if (!_indexors.ContainsKey(typeof(TDocument)))
            {
                _indexors.Add(typeof(TDocument), new DocumentIndexor<TDocument>(_provider));
            }

            return (DocumentIndexor<TDocument>)_indexors[typeof(TDocument)];
        }
    }
}
