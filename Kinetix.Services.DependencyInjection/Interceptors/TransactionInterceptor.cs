#pragma warning disable MA0042, KTA1104

using System.Diagnostics;
using AspectCore.DynamicProxy;

namespace Kinetix.Services.DependencyInjection.Interceptors;

/// <summary>
/// Intercepteur pour poser une transaction autour des appels de service.
/// </summary>
public class TransactionInterceptor(TransactionScopeManager transactionScopeManager) : AbstractInterceptor
{
    /// <inheritdoc cref="IInterceptor.Invoke" />
    [DebuggerNonUserCode]
    public override async Task Invoke(AspectContext context, AspectDelegate next)
    {
        if (Attribute.IsDefined(context.ServiceMethod, typeof(NoTransactionAttribute), inherit: true))
        {
            await next(context);
            return;
        }

        if (context.IsAsync())
        {
            await using var tx = await transactionScopeManager.EnsureTransactionAsync();
            await next(context);
            tx.Complete();
        }
        else
        {
            using var tx = transactionScopeManager.EnsureTransaction();
            await next(context);
            tx.Complete();
        }
    }
}
