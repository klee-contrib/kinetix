using Castle.DynamicProxy;

namespace Kinetix.Services.DependencyInjection.Interceptors;

/// <summary>
/// Intercepteur posant un contexte transactionnel.
/// </summary>
public class TransactionInterceptor(TransactionScopeManager transactionScopeManager) : IInterceptor
{
    /// <summary>
    /// Execution de l'intercepteur.
    /// </summary>
    /// <param name="invocation">Paramètres d'appel de la méthode cible.</param>
    public void Intercept(IInvocation invocation)
    {
        var noTransactionAttrs = invocation.Method.GetCustomAttributes<NoTransactionAttribute>(true);
        if (noTransactionAttrs.Length > 0)
        {
            invocation.Proceed();
            return;
        }

        using var tx = transactionScopeManager.EnsureTransaction();
        invocation.Proceed();
        tx.Complete();
    }
}
