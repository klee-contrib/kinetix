using System.Collections.Generic;
using System.Transactions;
using Castle.DynamicProxy;

namespace Kinetix.Services.DependencyInjection.Interceptors
{
    /// <summary>
    /// Intercepteur posant un contexte transactionnel.
    /// </summary>
    public class TransactionInterceptor : IInterceptor
    {
        private readonly IEnumerable<IOnBeforeCommit> _onBeforeCommits;
        private readonly TransactionScopeManager _transactionScopeManager;

        public TransactionInterceptor(IEnumerable<IOnBeforeCommit> onBeforeCommits, TransactionScopeManager transactionScopeManager)
        {
            _onBeforeCommits = onBeforeCommits;
            _transactionScopeManager = transactionScopeManager;
        }

        /// <summary>
        /// Execution de l'intercepteur.
        /// </summary>
        /// <param name="invocation">Paramètres d'appel de la méthode cible.</param>
        public void Intercept(IInvocation invocation)
        {
            if (Transaction.Current != null)
            {
                invocation.Proceed();
                return;
            }
            var noTransactionAttrs = invocation.Method.GetCustomAttributes<NoTransactionAttribute>(true);
            if (Transaction.Current == null && noTransactionAttrs.Length > 0)
            {
                invocation.Proceed();
                return;
            }

            using var tx = _transactionScopeManager.EnsureTransaction();

            invocation.Proceed();
            if (Transaction.Current.TransactionInformation.Status != TransactionStatus.Aborted)
            {
                foreach (var onBeforeCommit in _onBeforeCommits)
                {
                    onBeforeCommit.OnBeforeCommit();
                }

                tx.Complete();
            }
        }
    }
}
