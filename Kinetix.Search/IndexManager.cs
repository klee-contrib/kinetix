using Kinetix.Search.Config;
using Kinetix.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kinetix.Search;

/// <summary>
/// Gère la réindexation des documents.
/// </summary>
public class IndexManager
{
    private readonly ILogger<IndexManager> _logger;
    private readonly IServiceProvider _provider;
    private readonly ISearchStore _searchStore;
    private readonly TransactionScopeManager _transactionScopeManager;

    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="logger">Logger.</param>
    /// <param name="provider">Composant injecté.</param>
    /// <param name="searchStore">Composant injecté.</param>
    /// <param name="transactionScopeManager">Composant injecté.</param>
    public IndexManager(ILogger<IndexManager> logger, IServiceProvider provider, ISearchStore searchStore, TransactionScopeManager transactionScopeManager)
    {
        _logger = logger;
        _provider = provider;
        _searchStore = searchStore;
        _transactionScopeManager = transactionScopeManager;
    }

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
        GetContext().RegisterDelete<TDocument>(id);
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
        GetContext().RegisterDelete(bean);
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
        GetContext().RegisterIndex<TDocument>(id);
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
        GetContext().RegisterIndex(bean);
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
        GetContext().IndexAll<TDocument>();
        return this;
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
        using var tx = _transactionScopeManager.EnsureTransaction();

        var indexName = SearchConfig.GetTypeNameForIndex(typeof(TDocument));

        rebuildLogger?.LogInformation($"Index {indexName} rebuild started...");
        var indexCreated = _searchStore.EnsureIndex<TDocument>();
        if (indexCreated)
        {
            rebuildLogger?.LogInformation($"Index {indexName} (re)created.");
        }

        rebuildLogger?.LogInformation($"Loading data for index {indexName}...");
        var documents = _provider.GetService<IDocumentLoader<TDocument>>().GetAll(!indexCreated);
        rebuildLogger?.LogInformation($"Data for index {indexName} loaded.");
        if (documents is ICollection<TDocument> coll)
        {
            rebuildLogger?.LogInformation($"{coll.Count} documents ready for indexation.");
        }

        return _searchStore.ResetIndex(documents, !indexCreated, rebuildLogger);
    }

    private IndexingTransactionContext GetContext()
    {
        var context = _transactionScopeManager.ActiveScope?.GetContext<IndexingTransactionContext>();
        return context ?? throw new InvalidOperationException("Impossible d'enregistrer une réindexation en dehors d'un contexte de transaction.");
    }
}
