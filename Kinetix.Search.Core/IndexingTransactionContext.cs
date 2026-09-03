using System.Reflection;
using Kinetix.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kinetix.Search.Core;

internal class IndexingTransactionContext(IServiceProvider provider) : IAsyncTransactionContext
{
    private readonly Dictionary<Type, IIndexingDocumentState> _indexors = [];

    /// <inheritdoc />
    public bool Completed { get; set; }

    /// <inheritdoc />
    public TransactionContextStatus Status { get; set; }

    /// <summary>
    /// Attends le refresh de l'index lors du commit ou non. Par défaut: true.
    /// </summary>
    internal bool WaitForRefresh { get; set; } = true;

    /// <inheritdoc cref="IAsyncTransactionContext.InitAsync" />
    public Task InitAsync(CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc cref="IAsyncTransactionContext.OnAfterCommitAsync" />
    public Task OnAfterCommitAsync(CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc cref="IAsyncTransactionContext.OnBeforeCommitAsync" />
    public async Task OnBeforeCommitAsync(CancellationToken ct = default)
    {
        if (Completed && _indexors.Count != 0)
        {
            var searchStore = provider.GetRequiredService<ISearchStore>();
            var transactionScopeManager = provider.GetRequiredService<TransactionScopeManager>();
            var logger = provider.GetRequiredService<ILogger<IndexingTransactionContext>>();

            await using var tx = await transactionScopeManager.EnsureTransactionAsync(ct);

            var bulk = searchStore.Bulk();

            try
            {
                foreach (var indexor in _indexors)
                {
                    logger.LogInformation($"Prepare {indexor.Key.Name}");
                    await (Task<ISearchBulkDescriptor>)
                        typeof(IndexingTransactionContext)
                            .GetMethod(
                                nameof(PrepareBulkDescriptorAsync),
                                BindingFlags.Static | BindingFlags.NonPublic
                            )!
                            .MakeGenericMethod(indexor.Key)
                            .Invoke(null, [provider, bulk, indexor.Value, ct])!;
                }

                await bulk.RunAsync(WaitForRefresh, ct);
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

    /// <inheritdoc cref="IAsyncTransactionContext.OnCommitAsync" />
    public Task OnCommitAsync(CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }

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

    private static async Task<ISearchBulkDescriptor> PrepareBulkDescriptorAsync<TDocument>(
        IServiceProvider provider,
        ISearchBulkDescriptor bulk,
        IIndexingDocumentState state1,
        CancellationToken ct = default
    )
        where TDocument : class
    {
        var state = (IndexingDocumentState<TDocument>)state1;

        var loader = provider.GetRequiredService<IDocumentLoader<TDocument>>();

        if (state.Reindex)
        {
            var docs = await loader.GetAllAsync(partialRebuild: false, ct).ToListAsync(ct);
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
                var doc = await loader.GetAsync(state.IdsToIndex.Single(), ct);
                if (doc != null)
                {
                    bulk.Index(doc);
                }
            }
            else if (state.IdsToIndex.Count > 1)
            {
                var docs = await loader.GetManyAsync(state.IdsToIndex, ct).ToListAsync(cancellationToken: ct);

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
