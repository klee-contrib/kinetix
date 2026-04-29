namespace Kinetix.Services;

/// <summary>
/// Statut d'un contexte transactionnel dans une transaction en cours.
/// </summary>
public enum TransactionContextStatus
{
    NotStarted,
    Initialized,
    Handled,
}
