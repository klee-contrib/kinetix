namespace Kinetix.Services;

/// <summary>
/// Interface à implémenter pour gérer les notifs asynchrones sur les listes de référence (pour instances de caches partagées).
/// </summary>
public interface IAsyncReferenceNotifier : IReferenceNotifier
{
    /// <summary>
    /// Notifie un flush de liste de référence.
    /// </summary>
    /// <param name="referenceName">Le nom de la liste de référence.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Task.</returns>
    Task NotifyFlushAsync(string referenceName, CancellationToken ct = default);

    /// <summary>
    /// Enregistre un flush de liste de référence.
    /// </summary>
    /// <param name="referenceName">Le nom de la liste de référence.</param>
    /// <param name="flusher">Action qui flushe le cache mémoire.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Task.</returns>
    Task RegisterFlushAsync(string referenceName, Func<Task> flusher, CancellationToken ct = default);
}
