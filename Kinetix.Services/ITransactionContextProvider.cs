namespace Kinetix.Services;

/// <summary>
/// Provider à enregistrer pour fournir un contexte de transaction au manager.
/// </summary>
public interface ITransactionContextProvider
{
    /// <summary>
    /// Crée un nouveau contexte de transaction.
    /// </summary>
    /// <returns>Contexte de transaction.</returns>
    ITransactionContext Create();
}
