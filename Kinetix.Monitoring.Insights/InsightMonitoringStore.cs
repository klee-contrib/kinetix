using System.Collections.Concurrent;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace Kinetix.Monitoring.Insights;

public class InsightMonitoringStore : IMonitoringStore
{
    private readonly TelemetryClient _telemetryClient;

    /// <summary>
    /// Liste des Operations en court d'execution
    /// </summary>
    readonly ConcurrentDictionary<Guid, IOperationHolder<DependencyTelemetry>> _holders = new();

    public InsightMonitoringStore(TelemetryClient telemetryClient)
    {
        _telemetryClient = telemetryClient;
    }

    public void StartProcess(Guid id, string name, string category, string target = null)
    {
        if (category != "Service" && category != "Database")
        {
            return;
        }

        var processName = string.Join(".", name.Split('.').Reverse().Take(2).Reverse());

        if (category == "Service" && !processName.StartsWith("IService"))
        {
            return;
        }

        var holder = _telemetryClient.StartOperation<DependencyTelemetry>(processName);
        holder.Telemetry.Type = category;

        if (target != null)
        {
            holder.Telemetry.Target = target;
        }

        _holders.TryAdd(id, holder);
    }

    public void StopProcess(Guid id, bool success, bool disabled)
    {
        if (_holders.TryRemove(id, out var holder))
        {
            if (disabled)
            {
                holder.Telemetry.Properties["disabled"] = "true";
            }

            if (!success)
            {
                holder.Telemetry.Success = false;
                holder.Telemetry.ResultCode = "Failed";
            }

            _telemetryClient.StopOperation(holder);
        }
    }
}
