using System;
using Castle.DynamicProxy;
using Microsoft.Extensions.Logging;

namespace Kinetix.Services.DependencyInjection.Interceptors
{
    /// <summary>
    /// Intercepteur pour analytics + log.
    /// </summary>
    public class AnalyticsInterceptor : IInterceptor
    {
        private readonly ILogger<Service> _logger;
        private readonly ServicesAnalytics _serviceAnalytics;

        public AnalyticsInterceptor(ILogger<Service> logger, ServicesAnalytics serviceAnalytics)
        {
            _logger = logger;
            _serviceAnalytics = serviceAnalytics;
        }

        /// <summary>
        /// Invocation de la méthode, rajoute les advices nécessaires.
        /// </summary>
        /// <param name="invocation">Methode cible.</param>
        public void Intercept(IInvocation invocation)
        {
            var noAnalytics = invocation.Method.GetCustomAttributes<NoAnalyticsAttribute>(true).Length > 0;

            if (!noAnalytics)
            {
                _serviceAnalytics.StartService($"{invocation.Method.DeclaringType.FullName}.{invocation.Method.Name}");
            }

            try
            {
                invocation.Proceed();
                if (!noAnalytics)
                {
                    var duration = _serviceAnalytics.StopService();
                    _logger.LogInformation($"{invocation.Method.DeclaringType.FullName}.{invocation.Method.Name} ({duration} ms)");
                }
            }
            catch (Exception ex)
            {
                if (!noAnalytics)
                {
                    _serviceAnalytics.StopServiceInError();
                }

                _logger.LogError(ex, $"Erreur sur le service {invocation.Method.DeclaringType.FullName}.{invocation.Method.Name}");
                throw;
            }
        }
    }
}
