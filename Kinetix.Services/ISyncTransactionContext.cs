namespace Kinetix.Services;

/// <summary>
/// Définit un contexte de transaction synchrone a attacher à un scope de transaction.
/// </summary>
public interface ISyncTransactionContext : ITransactionContext
{
    /// <summary>
    /// Initialise le contexte.
    /// </summary>
    void Init();

    /// <summary>
    /// Action a exécuter après le commit du scope courant.
    /// </summary>
    void OnAfterCommit();

    /// <summary>
    /// Action a exécuter avant le commit du scope courant.
    /// </summary>
    void OnBeforeCommit();

    /// <summary>
    /// Action a exécuter lors du commit du scope courant.
    /// </summary>
    void OnCommit();
}
