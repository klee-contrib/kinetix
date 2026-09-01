#pragma warning disable S3877

using Microsoft.Extensions.Logging;

namespace Kinetix.Services;

public partial class ServiceScope : IDisposable
{
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

    public void Init()
    {
        foreach (var context in _contexts.OfType<ISyncTransactionContext>())
        {
            context.Init();
            context.Status = TransactionContextStatus.Initialized;
        }
    }
}
