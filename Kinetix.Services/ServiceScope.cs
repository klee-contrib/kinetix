namespace Kinetix.Services;

/// <summary>
/// Scope définissant une transaction en cours, muni de divers contextes transactionnels 
/// (exemple : une transaction ouverte en BDD est un contexte transactionnel).
/// </summary>
public class ServiceScope : IDisposable
{
    private readonly ITransactionContext[] _contexts;
    private readonly TransactionScopeManager _manager;

    internal ServiceScope()
    {
        _contexts = Array.Empty<ITransactionContext>();
    }

    internal ServiceScope(ITransactionContext[] contexts, TransactionScopeManager manager)
    {
        _contexts = contexts;
        _manager = manager;
    }

    /// <summary>
    /// Marque le scope comme étant valide.
    /// </summary>
    public void Complete()
    {
        foreach (var context in _contexts)
        {
            context.Complete();
        }
    }

    /// <summary>
    /// Libère le scope.
    /// </summary>
    public void Dispose()
    {
        foreach (var context in _contexts)
        {
            context.OnBeforeCommit();
        }

        foreach (var context in _contexts)
        {
            context.OnCommit();
        }

        _manager?.PopScope(this);

        foreach (var context in _contexts)
        {
            context.OnAfterCommit();
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
