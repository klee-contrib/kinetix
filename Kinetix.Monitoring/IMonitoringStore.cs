namespace Kinetix.Monitoring;

public interface IMonitoringStore
{
    /// <summary>
    /// Enregistre le démarrage d'un process
    /// </summary>
    /// <param name="id">Id du process.</param>
    /// <param name="name">Nom.</param>
    /// <param name="category">Catégorie.</param>
    /// <param name="target">Target.</param>
    void StartProcess(Guid id, string name, string category, string target = null);

    /// <summary>
    /// Enregistre la fin d'un process.
    /// </summary>
    /// <param name="id">Id du process.</param>
    /// <param name="success">Résultat.</param>
    /// <param name="disabled">Si désactivé.</param>
    void StopProcess(Guid id, bool success, bool disabled);
}
