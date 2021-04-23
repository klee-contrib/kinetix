using System;
using System.Collections.Generic;
using System.Linq;

namespace Kinetix.Monitoring
{
    public class AnalyticsManager
    {
        private readonly IEnumerable<IMonitoringStore> _stores;
        private readonly Stack<Process> _processes = new Stack<Process>();

        public AnalyticsManager(IEnumerable<IMonitoringStore> stores)
        {
            _stores = stores;
        }

        public void IncrementCounter(string code, int value)
        {
            if (_processes.Any())
            {
                _processes.Peek().IncrementValue(code, value);
            }
        }

        public void ResetCounters(IEnumerable<string> codes)
        {
            if (_processes.Any())
            {
                foreach (var code in codes)
                {
                    _processes.Peek().OwnCounters.Remove(code);
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
            var stoppedProcess = _processes.Pop();
            stoppedProcess.End = DateTime.Now;
            stoppedProcess.IsError = isError;

            if (_processes.Any())
            {
                _processes.Peek().SubProcesses.Add(stoppedProcess);
            }

            foreach (var store in _stores)
            {
                store.AddProcess(stoppedProcess);
            }

            return stoppedProcess.Duration.Value;
        }
    }
}
