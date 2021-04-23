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
        private readonly TelemetryClient _telemetryClient;

        /// <summary>
        /// Liste des Operations en court d'execution
        /// </summary>
        readonly ConcurrentDictionary<Guid, IOperationHolder<DependencyTelemetry>> _holders = new();

        public InsightMonitoringStore(TelemetryClient telemetryClient)
        {
            _telemetryClient = telemetryClient;
        }

        public void AddProcess(Process process)
        {
            var success = _holders.TryRemove(process.Id, out var holder);

            if (success)
            {
                if (process.IsError)
                {
                    holder.Telemetry.Success = false;
                    holder.Telemetry.ResultCode = "Failed";
                }

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
