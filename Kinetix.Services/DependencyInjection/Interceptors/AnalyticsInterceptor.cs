using System.Diagnostics;
using Castle.DynamicProxy;
using Kinetix.Monitoring.Core;
using Microsoft.Extensions.Logging;

namespace Kinetix.Services.DependencyInjection.Interceptors;

/// <summary>
/// Intercepteur pour analytics + log.
/// </summary>
public class AnalyticsInterceptor(ILogger<Service> logger, AnalyticsManager analytics) : IInterceptor
{
    /// <summary>
    /// Invocation de la méthode, rajoute les advices nécessaires.
    /// </summary>
    /// <param name="invocation">Methode cible.</param>
    [DebuggerNonUserCode]
    public void Intercept(IInvocation invocation)
    {
        analytics.StartProcess($"{invocation.Method.DeclaringType.FullName}.{invocation.Method.Name}", "Service");

        if (invocation.Method.GetCustomAttributes<NoAnalyticsAttribute>(true).Length > 0)
        {
            analytics.MarkProcessDisabled();
        }

        try
        {
            invocation.Proceed();
            var process = analytics.StopProcess();
            if (!process.Disabled)
            {
                logger.LogInformation(
                    $"{invocation.Method.DeclaringType.FullName}.{invocation.Method.Name} ({process.Duration} ms)"
                );
            }
        }
        catch (Exception ex)
        {
            analytics.MarkProcessInError();
            analytics.StopProcess();

            if (ex is AggregateException)
            {
                ex = ex.InnerException;
            }

            logger.LogError(
                ex,
                $"Erreur sur le service {invocation.Method.DeclaringType.FullName}.{invocation.Method.Name}"
            );
            throw new InterceptedException(
                $"Une erreur est survenue sur le service {invocation.Method.DeclaringType.FullName}.{invocation.Method.Name}",
                ex
            );
        }
    }
}
