namespace Kinetix.Services;

/// <summary>
/// Interface à implémenter pour gérer les notifs sur les listes de référence (si implé distribuée).
/// </summary>
public interface IReferenceNotifier : IDisposable
{
    /// <summary>
    /// Notifie un flush de liste de référence.
    /// </summary>
    /// <param name="referenceName">Liste de référence.</param>
    void NotifyFlush(string referenceName);

    /// <summary>
    /// Enregistre un flush de liste de référence.
    /// </summary>
    /// <param name="referenceName">Liste de référence.</param>
    /// <param name="flusher">Action qui flushe le cache mémoire.</param>
    void RegisterFlush(string referenceName, Action flusher);
}
