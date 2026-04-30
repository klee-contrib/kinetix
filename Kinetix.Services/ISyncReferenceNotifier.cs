namespace Kinetix.Services;

/// <summary>
/// Interface à implémenter pour gérer les notifs synchrones sur les listes de référence (pour instances de caches partagées).
/// </summary>
public interface ISyncReferenceNotifier : IReferenceNotifier
{
    /// <summary>
    /// Notifie un flush synchrone de liste de référence.
    /// </summary>
    /// <param name="referenceName">Le nom de la liste de référence.</param>
    /// <returns>Task.</returns>
    void NotifyFlush(string referenceName);

    /// <summary>
    /// Enregistre un flush synchrone de liste de référence.
    /// </summary>
    /// <param name="referenceName">Le nom de la liste de référence.</param>
    /// <param name="flusher">Action qui flushe le cache mémoire.</param>
    /// <returns>Task.</returns>
    void RegisterFlush(string referenceName, Action flusher);
}
