using System.Diagnostics;
using System.Reflection;
using AspectCore.DynamicProxy;
using Kinetix.Monitoring.Core;
using Microsoft.Extensions.Logging;

namespace Kinetix.Services.DependencyInjection.Interceptors;

/// <summary>
/// Intercepteur pour analytics + log.
/// </summary>
public class AnalyticsInterceptorAttribute(ILogger<Service> logger, AnalyticsManager analytics)
    : AbstractInterceptorAttribute
{
    /// <inheritdoc cref="IInterceptor.Invoke" />
    [DebuggerNonUserCode]
    public override async Task Invoke(AspectContext context, AspectDelegate next)
    {
        analytics.StartProcess(
            $"{context.ImplementationMethod.DeclaringType?.FullName}.{context.ImplementationMethod.Name}",
            "Service"
        );

        if (IsDefined(context.ImplementationMethod, typeof(NoAnalyticsAttribute), inherit: true))
        {
            analytics.MarkProcessDisabled();
        }

        try
        {
            await next(context);
            var process = analytics.StopProcess();
            if (process != null && !process.Disabled)
            {
                logger.LogInformation(
                    $"{context.ImplementationMethod.DeclaringType?.FullName}.{context.ImplementationMethod.Name} ({process.Duration} ms)"
                );
            }
        }
        catch (Exception ex)
        {
            analytics.MarkProcessInError();
            analytics.StopProcess();

            if (ex is AggregateException)
            {
                ex = ex.InnerException!;
            }

            logger.LogError(
                ex,
                $"Erreur sur le service {context.ImplementationMethod.DeclaringType?.FullName}.{context.ImplementationMethod.Name}"
            );
            throw new TargetInvocationException(
                $"Une erreur est survenue sur le service {context.ImplementationMethod.DeclaringType?.FullName}.{context.ImplementationMethod.Name}",
                ex
            );
        }
    }
}
