using System;

namespace Kinetix.Services
{
    /// <summary>
    /// Définit un contexte de transaction a attacher à un scope de transaction.
    /// </summary>
    public interface ITransactionContext : IDisposable
    {
        /// <summary>
        /// Contexte de BDD (disposé en premier, peut être omis)
        /// </summary>
        bool IsDatabaseContext { get; }

        /// <summary>
        /// Marque la transaction comme étant valide.
        /// </summary>
        void Complete();
    }
}
