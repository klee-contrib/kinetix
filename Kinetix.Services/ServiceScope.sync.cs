#pragma warning disable S3877

using Microsoft.Extensions.Logging;

namespace Kinetix.Services;

public partial class ServiceScope : IDisposable
{
    public bool Sync { get; set; }

    /// <summary>
    /// Libère le scope.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (!Sync)
        {
            throw new NotSupportedException(
                "Ce scope a été initialisé via une transaction asynchrone mais libéré de manière synchrone. Vérifiez que vous utilisez `DisposeAsync`/`await using`, et que votre scope de dépendances a été créé avec `CreateAsyncScope()`."
            );
        }

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

        _disposed = true;
    }

    public void Init()
    {
        var syncContexts = _contexts.OfType<ISyncTransactionContext>();

        var asyncOnlyContexts = _contexts.Except(syncContexts);
        if (asyncOnlyContexts.Any())
        {
            throw new NotSupportedException(
                $"Impossible de démarrer une transaction synchrone avec des contextes de transaction uniquement asynchrones : {string.Join(", ", asyncOnlyContexts.Select(c => c.GetType().Name))}"
            );
        }

        foreach (var context in syncContexts)
        {
            context.Init();
        }
    }
}
