using System;
using System.Collections.Generic;
using System.Linq;

namespace Kinetix.Services
{
    /// <summary>
    /// Manager de transactions.
    /// </summary>
    public class TransactionScopeManager : IDisposable
    {
        private readonly IEnumerable<ITransactionContextProvider> _contextProviders;
        private readonly Stack<ServiceScope> _scopes = new();

        public TransactionScopeManager(IEnumerable<ITransactionContextProvider> contextProviders)
        {
            _contextProviders = contextProviders;
        }

        /// <summary>
        /// Scope de transaction actif, avec ses contextes transactionnels.
        /// </summary>
        /// <remarks>(Un sous-scope créé par <see cref="EnsureTransaction"/> n'est pas le scope actif)</remarks>
        public ServiceScope ActiveScope => _scopes.Any() ? _scopes.Peek() : null;

        /// <summary>
        /// Débute une nouvelle transaction, indépendante d'une éventuelle transaction existante.
        /// </summary>
        /// <remarks>Cette transaction gérera ces propres contextes, comme ses connections en BDD par exemple.</remarks>
        /// <returns>Scope de la transaction.</returns>
        public ServiceScope BeginNewTransaction()
        {
            var scope = new ServiceScope(_contextProviders.Select(ctx => ctx.Create()).ToArray(), this);
            _scopes.Push(scope);
            return scope;
        }

        /// <summary>
        /// Vérifie la présence d'une transaction pré-existante, et la crée le cas échéant.
        /// </summary>
        /// <returns>Scope de la transaction (actif si la transaction a été créee).</returns>
        public ServiceScope EnsureTransaction()
        {
            return ActiveScope != null
                ? new ServiceScope()
                : BeginNewTransaction();
        }

        /// <summary>
        /// Libére tous les scopes de transactions (s'ils n'ont pas déjà été libérés).
        /// </summary>
        public void Dispose()
        {
            foreach (var scope in _scopes.ToList())
            {
                scope.Dispose();
            }
        }

        /// <summary>
        /// Retire le scope demandé de la pile de scope, s'il s'agit bien de lui.
        /// </summary>
        /// <param name="scope">Scope de transaction.</param>
        internal void PopScope(ServiceScope scope)
        {
            var activeScope = _scopes.Pop();
            if (activeScope != scope)
            {
                throw new InvalidOperationException("Erreur lors de la clôture d'une transaction");
            }
        }
    }
}
