using System.Transactions;
using Castle.DynamicProxy;

namespace KinetixCore.SqlServer
{
    /// <summary>
    /// Proxy for Transactional Attribute. This proxy manage transactions around methods executions.
    /// </summary>
    public class TransactionalProxy : IInterceptor
    {
        public TransactionalProxy()
        {
        }

        void IInterceptor.Intercept(IInvocation invocation)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Required))
            {
                invocation.Proceed();
                scope.Complete();
            }
        }
    }
}
