using Castle.DynamicProxy;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Transactions;

namespace KinetixCore.Database
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
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required))
            {
                invocation.Proceed();
                scope.Complete();
            }
        }
    }
}
