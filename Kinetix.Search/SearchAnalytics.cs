using System.Collections.Generic;
using Kinetix.Monitoring;

namespace Kinetix.Search
{
    public class SearchAnalytics : IAnalytics
    {
        /// <summary>
        /// Nom du compteur de requetes à ES.
        /// </summary>
        public const string CounterESRequestCount = "ES_REQUEST_COUNT";

        /// <summary>
        /// Nom du compteur d'erreurs ES.
        /// </summary>
        public const string CounterESErrorCount = "ES_ERROR_COUNT";

        /// <summary>
        /// Nom du compteur de documents indexés dans ES.
        /// </summary>
        public const string CounterESIndexCount = "ES_INDEX_COUNT";

        /// <summary>
        /// Nom du compteur de documents supprimés dans ES.
        /// </summary>
        public const string CounterESDeleteCount = "ES_DELETE_COUNT";


        private readonly AnalyticsManager _analyticsManager;

        public SearchAnalytics(AnalyticsManager analyticsManager = null)
        {
            _analyticsManager = analyticsManager;
        }

        public string Category => "Search";

        public IList<Counter> Counters { get; } = new List<Counter>
        {
            new Counter { Code = CounterESRequestCount, Label = "Requêtes ES", WarningThreshold = 20, CriticalThreshold = 30 },
            new Counter { Code = CounterESErrorCount, Label = "Erreurs ES", WarningThreshold = 0, CriticalThreshold = 0 },
            new Counter { Code = CounterESIndexCount, Label = "Documents ES indexés", WarningThreshold = 0, CriticalThreshold = 0 },
            new Counter { Code = CounterESDeleteCount, Label = "Documents ES supprimés", WarningThreshold = 0, CriticalThreshold = 0 }
        };

        public void CountIndex(int docCount)
        {
            _analyticsManager?.IncrementCounter(CounterESIndexCount, docCount);
        }

        public void CountDelete(int docCount)
        {
            _analyticsManager?.IncrementCounter(CounterESDeleteCount, docCount);
        }

        public void StartQuery(string commandName)
        {
            _analyticsManager?.IncrementCounter(CounterESRequestCount, 1);
            _analyticsManager?.StartProcess(commandName, Category);
        }

        public int StopQuery()
        {
            return _analyticsManager?.StopProcess() ?? 0;
        }

        public void StopQueryInError()
        {
            _analyticsManager?.IncrementCounter(CounterESErrorCount, 1);
            StopQuery();
        }
    }
}
