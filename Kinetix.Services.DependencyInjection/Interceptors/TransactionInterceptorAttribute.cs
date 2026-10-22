#pragma warning disable MA0042, KTA1104

using System.Diagnostics;
using AspectCore.DynamicProxy;

namespace Kinetix.Services.DependencyInjection.Interceptors;

/// <summary>
/// Intercepteur posant un contexte transactionnel.
/// </summary>
public class TransactionInterceptorAttribute(TransactionScopeManager transactionScopeManager)
    : AbstractInterceptorAttribute
{
    /// <inheritdoc cref="IInterceptor.Invoke" />
    [DebuggerNonUserCode]
    public override async Task Invoke(AspectContext context, AspectDelegate next)
    {
        if (IsDefined(context.ImplementationMethod, typeof(NoAnalyticsAttribute), inherit: true))
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
