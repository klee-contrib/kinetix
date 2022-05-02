using Microsoft.Extensions.Logging;

namespace Kinetix.Services;

/// <summary>
/// Scope définissant une transaction en cours, muni de divers contextes transactionnels 
/// (exemple : une transaction ouverte en BDD est un contexte transactionnel).
/// </summary>
public class ServiceScope : IDisposable
{
    private readonly ITransactionContext[] _contexts;
    private readonly ILogger<ServiceScope> _logger;
    private readonly TransactionScopeManager _manager;

    internal ServiceScope()
    {
        _contexts = Array.Empty<ITransactionContext>();
    }

    internal ServiceScope(ITransactionContext[] contexts, ILogger<ServiceScope> logger, TransactionScopeManager manager)
    {
        _contexts = contexts;
        _logger = logger;
        _manager = manager;
    }

    /// <summary>
    /// Marque le scope comme étant valide.
    /// </summary>
    public void Complete()
    {
        foreach (var context in _contexts)
        {
            context.Completed = true;
        }
    }

    /// <summary>
    /// Libère le scope.
    /// </summary>
    public void Dispose()
    {
        Exception onBeforeException = null;

        try
        {
            foreach (var context in _contexts)
            {
                context.OnBeforeCommit();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Une erreur est survenue lors de la préparation du commit de la transaction courante.");
            onBeforeException = ex;
            foreach (var context in _contexts)
            {
                context.Completed = false;
            }
        }

        foreach (var context in _contexts)
        {
            context.OnCommit();
        }

        _manager?.PopScope(this);

        if (onBeforeException != null)
        {
            throw onBeforeException;
        }

        try
        {
            foreach (var context in _contexts)
            {
                context.OnAfterCommit();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Une erreur est survenue lors d'une action après commit de la transaction courante.");
        }
    }

    /// <summary>
    /// Récupère le contexte transactionnel demandé.
    /// </summary>
    /// <typeparam name="T">Type du contexte.</typeparam>
    /// <returns>Le contexte.</returns>
    public T GetContext<T>()
        where T : ITransactionContext
    {
        return _contexts.OfType<T>().SingleOrDefault();
    }
}
