using System.Collections.Generic;
using Kinetix.Monitoring;

namespace Kinetix.Edm
{
    public class EdmAnalytics : IAnalytics
    {
        /// <summary>
        /// Nom du compteur de requetes à SP.
        /// </summary>
        public const string CounterSharePointRequestCount = "SHAREPOINT_REQUEST_COUNT";

        /// <summary>
        /// Nom du compteur d'erreurs SP.
        /// </summary>
        public const string CounterSharePointErrorCount = "SHAREPOINT_ERROR_COUNT";

        /// <summary>
        /// Nom du compteur de timeout base de SP.
        /// </summary>
        public const string CounterSharePointTimeoutCount = "SHAREPOINT_TIMEOUT_COUNT";

        private readonly AnalyticsManager _analyticsManager;

        public EdmAnalytics(AnalyticsManager analyticsManager = null)
        {
            _analyticsManager = analyticsManager;
        }

        public string Category => "Edm";

        public IList<Counter> Counters { get; } = new List<Counter>
        {
            new Counter { Code = CounterSharePointRequestCount, Label = "Requêtes SharePoint", WarningThreshold = 20, CriticalThreshold = 30 },
            new Counter { Code = CounterSharePointErrorCount, Label = "Erreurs SharePoint", WarningThreshold = 0, CriticalThreshold = 0 }
        };

        internal void CountError()
        {
            _analyticsManager?.IncrementCounter(CounterSharePointErrorCount, 1);
        }

        internal void StartQuery(string commandName)
        {
            _analyticsManager?.IncrementCounter(CounterSharePointRequestCount, 1);
            _analyticsManager?.StartProcess(commandName, Category);
        }

        internal int StopQuery()
        {
            return _analyticsManager?.StopProcess() ?? 0;
        }
    }
}
