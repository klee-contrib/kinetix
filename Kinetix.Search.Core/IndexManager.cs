using Kinetix.Services;
using Microsoft.Extensions.Logging;

namespace Kinetix.Search.Core;

/// <summary>
/// Gère la réindexation des documents.
/// </summary>
/// <remarks>
/// Constructeur.
/// </remarks>
/// <param name="logger">Logger.</param>
/// <param name="provider">Composant injecté.</param>
/// <param name="searchStore">Composant injecté.</param>
/// <param name="transactionScopeManager">Composant injecté.</param>
public class IndexManager(
    ILogger<IndexManager> logger,
    IServiceProvider provider,
    ISearchStore searchStore,
    TransactionScopeManager transactionScopeManager
)
{
    private bool _waitForRefresh = true;

    /// <summary>
    /// Attends le refresh de l'index lors du commit ou non. Par défaut: true.
    /// </summary>
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

    /// <summary>
    /// Instancie un IndexManager pour le document demandé.
    /// </summary>
    /// <typeparam name="TDocument">Type de document.</typeparam>
    /// <returns>IndexManager.</returns>
    public IndexManager<TDocument> For<TDocument>()
        where TDocument : class
    {
        return new IndexManager<TDocument>(logger, provider, searchStore, transactionScopeManager, _waitForRefresh);
    }
}
