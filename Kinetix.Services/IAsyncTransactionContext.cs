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
    Task Init(CancellationToken ct = default);

    /// <summary>
    /// Action a exécuter après le commit du scope courant.
    /// </summary>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Task.</returns>
    Task OnAfterCommit(CancellationToken ct = default);

    /// <summary>
    /// Action a exécuter avant le commit du scope courant.
    /// </summary>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Task.</returns>
    Task OnBeforeCommit(CancellationToken ct = default);

    /// <summary>
    /// Action a exécuter lors du commit du scope courant.
    /// </summary>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Task.</returns>
    Task OnCommit(CancellationToken ct = default);
}
