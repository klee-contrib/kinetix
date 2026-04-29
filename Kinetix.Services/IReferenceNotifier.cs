namespace Kinetix.Services;

/// <summary>
/// Interface à implémenter pour gérer les notifs sur les listes de référence (si implé distribuée).
/// </summary>
public interface IReferenceNotifier : IDisposable
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
    Task RegisterFlushAsync(string referenceName, Action flusher, CancellationToken ct = default);
}
