using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace Kinetix.Services
{
    /// <summary>
    /// Manager de transactions.
    /// </summary>
    public class TransactionScopeManager : IDisposable
    {
        private readonly Stack<ServiceScope> _scopes = new Stack<ServiceScope>();

        /// <summary>
        /// Scope de transaction actif, contenant les ressources de la transaction.
        /// </summary>
        /// <remarks>(Un sous-scope créé par <see cref="EnsureTransaction"/> n'est pas le scope actif)</remarks>
        public ServiceScope ActiveScope => _scopes.Peek();

        /// <summary>
        /// Débute une nouvelle transaction, indépendante d'une éventuelle transaction existante.
        /// </summary>
        /// <remarks>Cette transaction gérera ces propres ressources, comme ses connections en BDD par exemple.</remarks>
        /// <param name="scopeTimeout">Timeout de la transaction.</param>
        /// <returns>Scope de la transaction.</returns>
        public ServiceScope BeginNewTransaction(TimeSpan? scopeTimeout = null)
        {
            var scope = scopeTimeout.HasValue
                ? new ServiceScope(TransactionScopeOption.RequiresNew, scopeTimeout.Value)
                : new ServiceScope(TransactionScopeOption.RequiresNew);

            scope.Manager = this;

            _scopes.Push(scope);
            return scope;
        }

        /// <summary>
        /// Débute un nouveau scope sans transaction.
        /// </summary>
        /// <returns>Scope.</returns>
        public ServiceScope BeginSuppressed()
        {
            var scope = new ServiceScope(TransactionScopeOption.Suppress) { Manager = this };
            _scopes.Push(scope);
            return scope;
        }

        /// <summary>
        /// Vérifie la présence d'une transaction pré-existante, et la crée le cas échéant.
        /// </summary>
        /// <returns>Scope de la transaction (actif si la transaction a été créee).</returns>
        public ServiceScope EnsureTransaction()
        {
            var scope = new ServiceScope(TransactionScopeOption.Required);

            if (!_scopes.Any())
            {
                scope.Manager = this;
                _scopes.Push(scope);
            }

            return scope;
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
