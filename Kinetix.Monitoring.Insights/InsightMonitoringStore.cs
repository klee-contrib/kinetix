using System;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace Kinetix.Monitoring.Insights
{
    public class InsightMonitoringStore : IMonitoringStore
    {
        private TelemetryClient _telemetryClient;

        /// <summary>
        /// Liste des Operations en court d'execution
        /// </summary>
        ConcurrentDictionary<Guid, IOperationHolder<DependencyTelemetry>> _holders = new ConcurrentDictionary<Guid, IOperationHolder<DependencyTelemetry>>();

        public InsightMonitoringStore(TelemetryClient telemetryClient)
        {
            _telemetryClient = telemetryClient;
        }


        public void AddProcess(Process process)
        {
            var success = _holders.TryRemove(process.Id, out var holder);

            if (success)
            {
                holder.Telemetry.Success = !process.IsError;
                _telemetryClient.StopOperation(holder);
            }
        }

        public void StartProcess(Process process)
        {
            if (process.Category == "Service")
            {
                var processName = string.Join(".", process.Name.Split('.').Reverse().Take(2).Reverse());
                var holder = _telemetryClient.StartOperation<DependencyTelemetry>(processName);
                holder.Telemetry.Type = "InProc";
                _holders.TryAdd(process.Id, holder);
            }
        }
    }
}
