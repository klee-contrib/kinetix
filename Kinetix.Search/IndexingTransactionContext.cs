using System.Reflection;
using Kinetix.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kinetix.Search;

internal class IndexingTransactionContext : ITransactionContext
{
    private readonly Dictionary<Type, IIndexingDocumentState> _indexors = new();
    private readonly IServiceProvider _provider;

    private bool _ok;

    public IndexingTransactionContext(IServiceProvider provider)
    {
        _provider = provider;
    }

    /// <inheritdoc cref="ITransactionContext.Complete" />
    public void Complete()
    {
        _ok = true;
    }

    /// <inheritdoc cref="ITransactionContext.OnAfterCommit" />
    public void OnAfterCommit()
    {
    }

    /// <inheritdoc cref="ITransactionContext.OnBeforeCommit" />
    public void OnBeforeCommit()
    {
        if (_ok && _indexors.Any())
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
                    logger.LogInformation($"Prepare {indexor.Key.Name}");
                    typeof(IndexingTransactionContext).GetMethod(nameof(PrepareBulkDescriptor), BindingFlags.Static | BindingFlags.NonPublic)
                        .MakeGenericMethod(indexor.Key)
                        .Invoke(null, new object[] { _provider, bulk, indexor.Value });
                }

                bulk.Run();
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

    internal void IndexAll<TDocument>()
        where TDocument : class
    {
        GetState<TDocument>().Reindex = true;
    }

    internal bool RegisterDelete<TDocument>(int id)
        where TDocument : class
    {
        return GetState<TDocument>().RegisterDelete(id);
    }

    internal bool RegisterDelete<TDocument>(TDocument bean)
        where TDocument : class
    {
        return GetState<TDocument>().RegisterDelete(bean);
    }

    internal bool RegisterIndex<TDocument>(int id)
        where TDocument : class
    {
        return GetState<TDocument>().RegisterIndex(id);
    }

    internal bool RegisterIndex<TDocument>(TDocument bean)
        where TDocument : class
    {
        return GetState<TDocument>().RegisterIndex(bean);
    }


    /// <inheritDoc cref="IDocumentIndexor.PrepareBulkDescriptor" />
    private static ISearchBulkDescriptor PrepareBulkDescriptor<TDocument>(IServiceProvider provider, ISearchBulkDescriptor bulk, IIndexingDocumentState _state)
        where TDocument : class
    {
        var state = (IndexingDocumentState<TDocument>)_state;

        var loader = provider.GetService<IDocumentLoader<TDocument>>();
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
                bulk.Delete<TDocument>(state.IdsToDelete.Single().ToString());
            }
            else if (state.IdsToDelete.Count > 1)
            {
                bulk.DeleteMany<TDocument>(state.IdsToDelete.Select(id => id.ToString()));
            }

            if (state.BeansToDelete.Count == 1)
            {
                bulk.Delete(state.BeansToDelete.Single());
            }
            else if (state.BeansToDelete.Count > 1)
            {
                bulk.DeleteMany(state.BeansToDelete);
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

            if (state.BeansToIndex.Count == 1)
            {
                var doc = loader.Get(state.BeansToIndex.Single());
                if (doc != null)
                {
                    bulk.Index(doc);
                }
            }
            else if (state.BeansToIndex.Count > 1)
            {
                var docs = loader.GetMany(state.BeansToIndex).ToList();
                if (docs.Any())
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
