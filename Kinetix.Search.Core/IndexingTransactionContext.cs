using System.Reflection;
using Kinetix.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kinetix.Search.Core;

internal class IndexingTransactionContext : ITransactionContext
{
    private readonly Dictionary<(Type TDocument, Type TKey), IIndexingDocumentState> _indexors = new();
    private readonly IServiceProvider _provider;

    public IndexingTransactionContext(IServiceProvider provider)
    {
        _provider = provider;
    }

    /// <inheritdoc />
    public bool Completed { get; set; }

    /// <summary>
    /// Attends le refresh de l'index lors du commit ou non. Par défaut: true.
    /// </summary>
    internal bool WaitForRefresh { get; set; } = true;

    /// <inheritdoc cref="ITransactionContext.OnAfterCommit" />
    public void OnAfterCommit()
    {
    }

    /// <inheritdoc cref="ITransactionContext.OnBeforeCommit" />
    public void OnBeforeCommit()
    {
        if (Completed && _indexors.Any())
        {
            var searchStore = _provider.GetRequiredService<ISearchStore>();
            var transactionScopeManager = _provider.GetRequiredService<TransactionScopeManager>();
            var logger = _provider.GetRequiredService<ILogger<IndexingTransactionContext>>();

            using var tx = transactionScopeManager.EnsureTransaction();

            var bulk = searchStore.Bulk();

            try
            {
                foreach (var indexor in _indexors)
                {
                    logger.LogInformation($"Prepare {indexor.Key.TDocument.Name}");
                    typeof(IndexingTransactionContext).GetMethod(nameof(PrepareBulkDescriptor), BindingFlags.Static | BindingFlags.NonPublic)
                        .MakeGenericMethod(indexor.Key.TDocument, indexor.Key.TKey)
                        .Invoke(null, new object[] { _provider, bulk, indexor.Value });
                }

                bulk.Run(WaitForRefresh);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error while indexing : ");
                throw;
            }

            _indexors.Clear();
            tx.Complete();
        }
    }

    /// <inheritdoc cref="ITransactionContext.OnCommit" />
    public void OnCommit()
    {
    }

    internal void IndexAll<TDocument, TKey>()
        where TDocument : class
    {
        GetState<TDocument, TKey>().Reindex = true;
    }

    internal bool RegisterDelete<TDocument, TKey>(TKey id)
        where TDocument : class
    {
        return GetState<TDocument, TKey>().RegisterDelete(id);
    }

    internal bool RegisterIndex<TDocument, TKey>(TKey id)
        where TDocument : class
    {
        return GetState<TDocument, TKey>().RegisterIndex(id);
    }

    private static ISearchBulkDescriptor PrepareBulkDescriptor<TDocument, TKey>(IServiceProvider provider, ISearchBulkDescriptor bulk, IIndexingDocumentState _state)
        where TDocument : class, new()
    {
        var state = (IndexingDocumentState<TDocument, TKey>)_state;

        var loader = provider.GetRequiredService<IDocumentLoader<TDocument, TKey>>();

        if (state.Reindex)
        {
            var docs = loader.GetAll(false).ToList();
            return docs.Any()
                ? bulk.IndexMany(docs)
                : bulk;
        }
        else
        {
            if (state.IdsToDelete.Count == 1)
            {
                bulk.Delete(loader.FillDocumentWithKey(state.IdsToDelete.Single()));
            }
            else if (state.IdsToDelete.Count > 1)
            {
                bulk.DeleteMany(state.IdsToDelete.Select(loader.FillDocumentWithKey));
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
                if (docs.Any())
                {
                    bulk.IndexMany(docs);
                }
            }

            return bulk;
        }
    }

    private IndexingDocumentState<TDocument, TKey> GetState<TDocument, TKey>()
        where TDocument : class
    {
        if (!_indexors.ContainsKey((typeof(TDocument), typeof(TKey))))
        {
            _indexors.Add((typeof(TDocument), typeof(TKey)), new IndexingDocumentState<TDocument, TKey>());
        }

        return (IndexingDocumentState<TDocument, TKey>)_indexors[(typeof(TDocument), typeof(TKey))];
    }
}
