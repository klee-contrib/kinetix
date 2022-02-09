using System.Collections.Generic;
using Kinetix.Monitoring;

namespace Kinetix.Data.SqlClient
{
    public class SqlServerAnalytics : IAnalytics
    {
        public const string CounterSqlRequestCount = "SQL_REQUEST_COUNT";
        public const string CounterSqlErrorCount = "SQL_ERROR_COUNT";
        public const string CounterSqlDeadLockCount = "SQL_DEADLOCK_COUNT";
        public const string CounterSqlTimeoutCount = "SQL_TIMEOUT_COUNT";

        private readonly AnalyticsManager _analyticsManager;

        public SqlServerAnalytics(AnalyticsManager analyticsManager = null)
        {
            _analyticsManager = analyticsManager;
        }

        public string Category => "Database";

        public IList<Counter> Counters { get; } = new List<Counter>
        {
            new Counter { Code = CounterSqlRequestCount, Label = "Nombre de requêtes", WarningThreshold = 20, CriticalThreshold = 30 },
            new Counter { Code = CounterSqlErrorCount, Label = "Nombre de requêtes en erreur", WarningThreshold = 0, CriticalThreshold = 0 },
            new Counter { Code = CounterSqlDeadLockCount, Label = "Nombre de requêtes en erreur de deadlock", WarningThreshold = 0, CriticalThreshold = 0 },
            new Counter { Code = CounterSqlTimeoutCount, Label = "Nombre de requêtes en erreur de timeout", WarningThreshold = 0, CriticalThreshold = 0 }
        };


        internal void CountDeadlock()
        {
            _analyticsManager?.IncrementCounter(CounterSqlDeadLockCount, 1);
        }

        internal void CountError()
        {
            _analyticsManager?.IncrementCounter(CounterSqlErrorCount, 1);
        }

        internal void CountTimeout()
        {
            _analyticsManager?.IncrementCounter(CounterSqlTimeoutCount, 1);
        }

        internal void StartCommand(string commandName)
        {
            _analyticsManager?.IncrementCounter(CounterSqlRequestCount, 1);
            _analyticsManager?.StartProcess(commandName, Category);
        }

        internal int StopCommand()
        {
            return _analyticsManager?.StopProcess() ?? 0;
        }
    }
}
