namespace Kinetix.Services;

/// <summary>
/// Définit un contexte de transaction asynchrone a attacher à un scope de transaction.
/// </summary>
public interface IAsyncTransactionContext : ITransactionContext
{
    /// <summary>
    /// Initialise le contexte.
    /// </summary>
    /// <param name="ct">CancellationToken.</param>
    Task InitAsync(CancellationToken ct = default);

    /// <summary>
    /// Action a exécuter après le commit du scope courant.
    /// </summary>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Task.</returns>
    Task OnAfterCommitAsync(CancellationToken ct = default);

    /// <summary>
    /// Action a exécuter avant le commit du scope courant.
    /// </summary>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Task.</returns>
    Task OnBeforeCommitAsync(CancellationToken ct = default);

    /// <summary>
    /// Action a exécuter lors du commit du scope courant.
    /// </summary>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Task.</returns>
    Task OnCommitAsync(CancellationToken ct = default);
}
