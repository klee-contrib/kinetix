using System;
using Microsoft.Extensions.DependencyInjection;

namespace Kinetix.Monitoring
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddMonitoring(this IServiceCollection services, Action<MonitoringConfig> action = null)
        {
            var config = new MonitoringConfig(services);
            action?.Invoke(config);
            return services.AddScoped<AnalyticsManager>();
        }
    }
}