using System.Reflection;
using Kinetix.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kinetix.Search.Core;

internal class IndexingTransactionContext(IServiceProvider provider) : ITransactionContext
{
    private readonly Dictionary<Type, IIndexingDocumentState> _indexors = [];

    /// <inheritdoc />
    public bool Completed { get; set; }

    /// <summary>
    /// Attends le refresh de l'index lors du commit ou non. Par défaut: true.
    /// </summary>
    internal bool WaitForRefresh { get; set; } = true;

    /// <inheritdoc cref="ITransactionContext.OnAfterCommit" />
    public void OnAfterCommit() { }

    /// <inheritdoc cref="ITransactionContext.OnBeforeCommit" />
    public void OnBeforeCommit()
    {
        if (Completed && _indexors.Count != 0)
        {
            var searchStore = provider.GetRequiredService<ISearchStore>();
            var transactionScopeManager = provider.GetRequiredService<TransactionScopeManager>();
            var logger = provider.GetRequiredService<ILogger<IndexingTransactionContext>>();

            using var tx = transactionScopeManager.EnsureTransaction();

            var bulk = searchStore.Bulk();

            try
            {
                foreach (var indexor in _indexors)
                {
                    logger.LogInformation($"Prepare {indexor.Key.Name}");
                    typeof(IndexingTransactionContext)
                        .GetMethod(nameof(PrepareBulkDescriptor), BindingFlags.Static | BindingFlags.NonPublic)!
                        .MakeGenericMethod(indexor.Key)
                        .Invoke(null, [provider, bulk, indexor.Value]);
                }

                bulk.Run(WaitForRefresh);
            }
#pragma warning disable S2139
            catch (Exception e)
            {
                logger.LogError(e, "Error while indexing : ");
                throw;
            }
#pragma warning restore S2139

            _indexors.Clear();
            tx.Complete();
        }
    }

    /// <inheritdoc cref="ITransactionContext.OnCommit" />
    public void OnCommit() { }

    internal void IndexAll<TDocument>()
        where TDocument : class
    {
        GetState<TDocument>().Reindex = true;
    }

    internal bool RegisterDelete<TDocument>(object id)
        where TDocument : class
    {
        return GetState<TDocument>().RegisterDelete(id);
    }

    internal bool RegisterIndex<TDocument>(object id)
        where TDocument : class
    {
        return GetState<TDocument>().RegisterIndex(id);
    }

    private static ISearchBulkDescriptor PrepareBulkDescriptor<TDocument>(
        IServiceProvider provider,
        ISearchBulkDescriptor bulk,
        IIndexingDocumentState state1
    )
        where TDocument : class
    {
        var state = (IndexingDocumentState<TDocument>)state1;

        var loader = provider.GetRequiredService<IDocumentLoader<TDocument>>();

        if (state.Reindex)
        {
            var docs = loader.GetAll(partialRebuild: false).ToList();
            return docs.Count != 0 ? bulk.IndexMany(docs) : bulk;
        }
        else
        {
            if (state.IdsToDelete.Count == 1)
            {
                bulk.Delete<TDocument>(state.IdsToDelete.Single());
            }
            else if (state.IdsToDelete.Count > 1)
            {
                bulk.DeleteMany<TDocument>(state.IdsToDelete);
            }

            if (state.IdsToIndex.Count == 1)
            {
                var doc = loader.Get(state.IdsToIndex.Single());
                if (doc != null)
                {
                    bulk.Index(doc);
                }
            }
            else if (state.IdsToIndex.Count > 1)
            {
                var docs = loader.GetMany(state.IdsToIndex).ToList();

                if (docs.Count != 0)
                {
                    bulk.IndexMany(docs);
                }
            }

            return bulk;
        }
    }

    private IndexingDocumentState<TDocument> GetState<TDocument>()
        where TDocument : class
    {
        if (!_indexors.ContainsKey(typeof(TDocument)))
        {
            _indexors.Add(typeof(TDocument), new IndexingDocumentState<TDocument>());
        }

        return (IndexingDocumentState<TDocument>)_indexors[typeof(TDocument)];
    }
}
