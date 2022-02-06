namespace Kinetix.Services;

/// <summary>
/// Définit un contexte de transaction a attacher à un scope de transaction.
/// </summary>
public interface ITransactionContext
{
    /// <summary>
    /// Marque la transaction comme étant valide.
    /// </summary>
    void Complete();

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
