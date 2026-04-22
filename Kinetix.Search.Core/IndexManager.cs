using Kinetix.Search.Core.Config;
using Kinetix.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kinetix.Search.Core;

/// <summary>
/// Implémentation de IIndexManager.
/// </summary>
internal class IndexManager(
    ILogger<IndexManager> logger,
    IServiceProvider provider,
    ISearchStore searchStore,
    TransactionScopeManager transactionScopeManager
) : IIndexManager
{
    private bool _waitForRefresh = true;

    /// <inheritdoc />
    public bool WaitForRefresh
    {
        get => _waitForRefresh;
        set
        {
            _waitForRefresh = value;
            var context = transactionScopeManager.ActiveScope?.GetContext<IndexingTransactionContext>();
            if (context != null)
            {
                context.WaitForRefresh = value;
            }
        }
    }

    /// <inheritdoc cref="IIndexManager.Delete{TDocument, TKey}" />
    public IIndexManager Delete<TDocument, TKey>(TKey id)
        where TDocument : class
        where TKey : notnull
    {
        logger.LogInformation($"RegisterDelete 1 {typeof(TDocument).Name}");
        GetContext().RegisterDelete<TDocument>(id);
        return this;
    }

    /// <inheritdoc cref="IIndexManager.DeleteMany{TDocument, TKey}" />
    public IIndexManager DeleteMany<TDocument, TKey>(IEnumerable<TKey> ids)
        where TDocument : class
        where TKey : notnull
    {
        logger.LogInformation($"RegisterDelete {ids.Count()} {typeof(TDocument).Name}");
        foreach (var id in ids)
        {
            GetContext().RegisterDelete<TDocument>(id);
        }

        return this;
    }

    /// <inheritdoc cref="IIndexManager.Index{TDocument, TKey}" />
    public IIndexManager Index<TDocument, TKey>(TKey id)
        where TDocument : class
        where TKey : notnull
    {
        logger.LogInformation($"RegisterIndex 1 {typeof(TDocument).Name}");
        GetContext().RegisterIndex<TDocument>(id);
        return this;
    }

    /// <inheritdoc cref="IIndexManager.IndexAll{TDocument}" />
    public IIndexManager IndexAll<TDocument>()
        where TDocument : class
    {
        logger.LogInformation($"Reindex {typeof(TDocument).Name}");
        GetContext().IndexAll<TDocument>();
        return this;
    }

    /// <inheritdoc cref="IIndexManager.IndexMany{TDocument, TKey}" />
    public IIndexManager IndexMany<TDocument, TKey>(IEnumerable<TKey> ids)
        where TDocument : class
        where TKey : notnull
    {
        logger.LogInformation($"RegisterIndex {ids.Count()} {typeof(TDocument).Name}");
        foreach (var id in ids)
        {
            GetContext().RegisterIndex<TDocument>(id);
        }

        return this;
    }

    /// <inheritdoc cref="IIndexManager.RebuildIndex{TDocument}" />
    public int RebuildIndex<TDocument>(ILogger? rebuildLogger = null, bool forcePartialRebuild = false)
        where TDocument : class
    {
        using var tx = transactionScopeManager.EnsureTransaction();

        var indexName = SearchConfig.GetTypeNameForIndex(typeof(TDocument));

        rebuildLogger?.LogInformation($"Index {indexName} rebuild started...");
        var indexCreated = searchStore.EnsureIndex<TDocument>();
        if (indexCreated)
        {
            rebuildLogger?.LogInformation($"Index {indexName} (re)created.");
        }

        var partialRebuild = !indexCreated || forcePartialRebuild;

        rebuildLogger?.LogInformation($"Loading data for index {indexName}...");

        var loader = provider.GetRequiredService<IDocumentLoader<TDocument>>();

        var documents = loader.GetAll(partialRebuild);
        rebuildLogger?.LogInformation($"Data for index {indexName} loaded.");

        return searchStore.ResetIndex(documents, partialRebuild, rebuildLogger);
    }

    private IndexingTransactionContext GetContext()
    {
        var context = transactionScopeManager.ActiveScope?.GetContext<IndexingTransactionContext>();

        if (context != null)
        {
            context.WaitForRefresh = _waitForRefresh;
            return context;
        }

        throw new InvalidOperationException(
            "Impossible d'enregistrer une réindexation en dehors d'un contexte de transaction."
        );
    }
}
