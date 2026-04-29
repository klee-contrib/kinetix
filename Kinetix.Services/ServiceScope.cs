#pragma warning disable S3877, KTA1104

using Microsoft.Extensions.Logging;

namespace Kinetix.Services;

/// <summary>
/// Scope définissant une transaction en cours, muni de divers contextes transactionnels
/// (exemple : une transaction ouverte en BDD est un contexte transactionnel).
/// </summary>
public class ServiceScope : IAsyncDisposable, IDisposable
{
    private readonly ITransactionContext[] _contexts;
    private readonly ILogger<ServiceScope>? _logger;
    private readonly TransactionScopeManager? _manager;

    internal ServiceScope()
    {
        _contexts = [];
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
        var contexts = _contexts.OfType<ISyncTransactionContext>();

        Exception? onBeforeException = null;

        try
        {
            foreach (var context in contexts)
            {
                context.OnBeforeCommit();
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(
                ex,
                "Une erreur est survenue lors de la préparation du commit de la transaction courante."
            );
            onBeforeException = ex;
            foreach (var context in contexts)
            {
                context.Completed = false;
            }
        }

        foreach (var context in contexts)
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
            foreach (var context in contexts)
            {
                context.OnAfterCommit();
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Une erreur est survenue lors d'une action après commit de la transaction courante.");
        }

        foreach (var context in contexts)
        {
            context.Status = TransactionContextStatus.Handled;
        }

        if (_contexts.Any(c => c.Status == TransactionContextStatus.Initialized))
        {
            throw new NotSupportedException(
                "Ce scope a été initialisé via une transaction asynchrone mais disposé de manière synchrone. Veuillez utiliser `DisposeAsync`/`await using` à la place."
            );
        }
    }

    /// <summary>
    /// Libère le scope.
    /// </summary>
    /// <returns>Task.</returns>
    public async ValueTask DisposeAsync()
    {
        Exception? onBeforeException = null;

        var contexts = _contexts.Where(c => c.Status == TransactionContextStatus.Initialized);

        try
        {
            foreach (var context in contexts)
            {
                switch (context)
                {
                    case IAsyncTransactionContext asyncContext:
                        await asyncContext.OnBeforeCommit();
                        break;
                    case ISyncTransactionContext syncContext:
                        syncContext.OnBeforeCommit();
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(
                ex,
                "Une erreur est survenue lors de la préparation du commit de la transaction courante."
            );
            onBeforeException = ex;
            foreach (var context in contexts)
            {
                context.Completed = false;
            }
        }

        foreach (var context in contexts)
        {
            switch (context)
            {
                case IAsyncTransactionContext asyncContext:
                    await asyncContext.OnCommit();
                    break;
                case ISyncTransactionContext syncContext:
                    syncContext.OnCommit();
                    break;
            }
        }

        _manager?.PopScope(this);

        if (onBeforeException != null)
        {
            throw onBeforeException;
        }

        try
        {
            foreach (var context in contexts)
            {
                switch (context)
                {
                    case IAsyncTransactionContext asyncContext:
                        await asyncContext.OnAfterCommit();
                        break;
                    case ISyncTransactionContext syncContext:
                        syncContext.OnAfterCommit();
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Une erreur est survenue lors d'une action après commit de la transaction courante.");
        }

        foreach (var context in contexts)
        {
            context.Status = TransactionContextStatus.Handled;
        }
    }

    /// <summary>
    /// Récupère le contexte transactionnel demandé.
    /// </summary>
    /// <typeparam name="T">Type du contexte.</typeparam>
    /// <returns>Le contexte.</returns>
    public T? GetContext<T>()
        where T : ITransactionContext
    {
        return _contexts.OfType<T>().SingleOrDefault();
    }

    public void Init()
    {
        foreach (var context in _contexts.OfType<ISyncTransactionContext>())
        {
            context.Init();
            context.Status = TransactionContextStatus.Initialized;
        }
    }

    public async Task InitAsync(CancellationToken ct = default)
    {
        foreach (var context in _contexts)
        {
            switch (context)
            {
                case IAsyncTransactionContext asyncContext:
                    await asyncContext.Init(ct);
                    break;
                case ISyncTransactionContext syncContext:
                    syncContext.Init();
                    break;
            }
            context.Status = TransactionContextStatus.Initialized;
        }
    }
}
