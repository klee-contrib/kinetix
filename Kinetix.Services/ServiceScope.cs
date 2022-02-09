using System;
using System.Collections.Generic;
using System.Transactions;

namespace Kinetix.Services
{
    /// <summary>
    /// Permet de créer un nouveau contexte transactionnel.
    /// </summary>
    public class ServiceScope : IDisposable
    {
        private readonly TransactionScope _scope;

        /// <summary>
        /// Crée une nouvelle transaction.
        /// </summary>
        /// <param name="scopeOption">Option.</param>
        internal ServiceScope(TransactionScopeOption scopeOption)
        {
            _scope = scopeOption switch
            {
                TransactionScopeOption.Required => Transaction.Current == null
                    ? new TransactionScope(scopeOption, new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted, })
                    : new TransactionScope(scopeOption),
                TransactionScopeOption.RequiresNew => new TransactionScope(scopeOption, new TransactionOptions() { IsolationLevel = IsolationLevel.ReadCommitted, }),
                TransactionScopeOption.Suppress => new TransactionScope(scopeOption),
                _ => throw new NotSupportedException(),
            };
        }

        /// <summary>
        /// Ressources.
        /// </summary>
        public IList<IDisposable> Resources { get; } = new List<IDisposable>();

        /// <summary>
        /// Manager.
        /// </summary>

        internal ServiceScopeManager Manager { get; set; }

        /// <summary>
        /// Crée une nouvelle transaction.
        /// </summary>
        /// <param name="scopeOption">Option.</param>
        /// <param name="scopeTimeout">Timeout.</param>
        internal ServiceScope(TransactionScopeOption scopeOption, TimeSpan scopeTimeout)
        {
            _scope = scopeOption switch
            {
                TransactionScopeOption.Required => Transaction.Current == null
                    ? new TransactionScope(scopeOption, new TransactionOptions() { Timeout = scopeTimeout, IsolationLevel = IsolationLevel.ReadCommitted, })
                    : new TransactionScope(scopeOption, new TransactionOptions() { Timeout = scopeTimeout, IsolationLevel = IsolationLevel.Unspecified }),
                TransactionScopeOption.RequiresNew => new TransactionScope(scopeOption, new TransactionOptions() { Timeout = scopeTimeout, IsolationLevel = IsolationLevel.ReadCommitted, }),
                TransactionScopeOption.Suppress => new TransactionScope(scopeOption, new TransactionOptions() { Timeout = scopeTimeout }),
                _ => throw new NotSupportedException(),
            };
        }

        /// <summary>
        /// Termine le scope avec succès.
        /// </summary>
        public void Complete()
        {
            _scope.Complete();
        }

        /// <summary>
        /// Libère le contexte.
        /// </summary>
        public void Dispose()
        {
            Manager?.PopScope(this);
            _scope.Dispose();
            foreach (var resource in Resources)
            {
                resource.Dispose();
            }
        }
    }
}
