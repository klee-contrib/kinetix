#pragma warning disable S3877

using Microsoft.Extensions.Logging;

namespace Kinetix.Services;

public partial class ServiceScope : IDisposable
{
    private bool _sync;

    /// <summary>
    /// Libère le scope.
    /// </summary>
    public void Dispose()
    {
        if (!_sync)
        {
            throw new NotSupportedException(
                "Ce scope a été initialisé via une transaction asynchrone mais disposé de manière synchrone. Veuillez utiliser `DisposeAsync`/`await using` à la place."
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
    }

    public void Init()
    {
        _sync = true;

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
