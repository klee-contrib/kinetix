using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Kinetix.Monitoring
{
    public class AnalyticsManager
    {
        private readonly IEnumerable<IMonitoringStore> _stores;
        private readonly ConcurrentStack<Process> _processes = new ConcurrentStack<Process>();

        public AnalyticsManager(IEnumerable<IMonitoringStore> stores)
        {
            _stores = stores;
        }

        public void IncrementCounter(string code, int value)
        {
            if (_processes.Any())
            {
                _processes.TryPeek(out var peek);
                peek.IncrementValue(code, value);
            }
        }

        public void ResetCounters(IEnumerable<string> codes)
        {
            if (_processes.Any())
            {
                foreach (var code in codes)
                {
                    _processes.TryPeek(out var peek);
                    peek.OwnCounters.Remove(code);
                }
            }
        }

        public void StartProcess(string name, string category)
        {
            var process = new Process(name, category);
            _processes.Push(process);

            foreach (var store in _stores)
            {
                store.StartProcess(process);
            }
        }

        public int StopProcess(bool isError = false)
        {
            _processes.TryPop(out var stoppedProcess);
            stoppedProcess.End = DateTime.Now;
            stoppedProcess.IsError = isError;

            if (_processes.Any())
            {
                _processes.TryPeek(out var peek);
                peek.SubProcesses.Add(stoppedProcess);
            }

            foreach (var store in _stores)
            {
                store.AddProcess(stoppedProcess);
            }

            return stoppedProcess.Duration.Value;
        }
    }
}
