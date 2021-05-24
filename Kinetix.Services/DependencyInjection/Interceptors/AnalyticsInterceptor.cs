using System;
using Castle.DynamicProxy;
using Kinetix.Monitoring;
using Microsoft.Extensions.Logging;

namespace Kinetix.Services.DependencyInjection.Interceptors
{
    /// <summary>
    /// Intercepteur pour analytics + log.
    /// </summary>
    public class AnalyticsInterceptor : IInterceptor
    {
        private readonly ILogger<Service> _logger;
        private readonly AnalyticsManager _analytics;

        public AnalyticsInterceptor(ILogger<Service> logger, AnalyticsManager analytics)
        {
            _logger = logger;
            _analytics = analytics;
        }

        /// <summary>
        /// Invocation de la méthode, rajoute les advices nécessaires.
        /// </summary>
        /// <param name="invocation">Methode cible.</param>
        public void Intercept(IInvocation invocation)
        {
            _analytics.StartProcess($"{invocation.Method.DeclaringType.FullName}.{invocation.Method.Name}", "Service");

            if (invocation.Method.GetCustomAttributes<NoAnalyticsAttribute>(true).Length > 0)
            {
                _analytics.MarkProcessDisabled();
            }

            try
            {
                invocation.Proceed();
                var process = _analytics.StopProcess();
                if (!process.Disabled)
                {
                    _logger.LogInformation($"{invocation.Method.DeclaringType.FullName}.{invocation.Method.Name} ({process.Duration} ms)");
                }
            }
            catch (Exception ex)
            {
                _analytics.MarkProcessInError();
                _analytics.StopProcess();

                _logger.LogError(ex, $"Erreur sur le service {invocation.Method.DeclaringType.FullName}.{invocation.Method.Name}");
                throw;
            }
        }
    }
}
