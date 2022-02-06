using Microsoft.ApplicationInsights;
using Microsoft.Extensions.DependencyInjection;

namespace Kinetix.Monitoring.Insights;

/// <summary>
/// Extension yolo
/// </summary>
public static class MonitoringInsightExtensions
{
    /// <summary>
    /// Ajoute un MonitoringStore branché à Application Insights
    /// </summary>
    /// <param name="config">Config monitoring</param>
    /// <returns>Config monitoring</returns>
    public static MonitoringConfig AddInsights(this MonitoringConfig config)
    {
        return config.AddStore(p => new InsightMonitoringStore(p.GetService<TelemetryClient>()));
    }
}
