using System.Collections.Generic;
using Kinetix.Monitoring;

namespace Kinetix.Services
{
    public class ServicesAnalytics : IAnalytics
    {
        public const string CounterTotalServiceCallCount = "TOTAL_SERVICE_CALL_COUNT";
        public const string CounterTotalServiceErrorCount = "TOTAL_SERVICE_ERROR_COUNT";

        private readonly AnalyticsManager _analyticsManager;

        public ServicesAnalytics(AnalyticsManager analyticsManager = null)
        {
            _analyticsManager = analyticsManager;
        }

        public string Category => "Service";

        public IList<Counter> Counters { get; } = new List<Counter>
        {
            new Counter { Code = CounterTotalServiceCallCount, Label = "Nb srv", WarningThreshold = 10, CriticalThreshold = 100 },
            new Counter { Code = CounterTotalServiceErrorCount, Label = "Exception src (%)", WarningThreshold = 10, CriticalThreshold = 50 }
        };

        internal void StartService(string serviceName)
        {
            _analyticsManager?.IncrementCounter(CounterTotalServiceCallCount, 1);
            _analyticsManager?.StartProcess(serviceName, Category);
        }

        internal int StopService(bool isError = false)
        {
            return _analyticsManager?.StopProcess(isError) ?? 0;
        }

        internal void StopServiceInError()
        {
            _analyticsManager?.IncrementCounter(CounterTotalServiceErrorCount, 1);
            StopService(true);
        }
    }
}
