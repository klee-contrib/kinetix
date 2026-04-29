namespace Kinetix.Services;

/// <summary>
/// Définit un contexte de transaction a attacher à un scope de transaction.
/// </summary>
public interface ITransactionContext
{
    /// <summary>
    /// Marque la transaction comme étant valide.
    /// </summary>
    bool Completed { get; set; }

    /// <summary>
    /// Statut du contexte transactionnel, pour savoir s'il a été initialisé et traité dans la transaction.
    /// </summary>
    TransactionContextStatus Status { get; set; }
}
