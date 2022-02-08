using System.Collections.Concurrent;

namespace Kinetix.Monitoring.Core;

public class AnalyticsManager
{
    private readonly IEnumerable<IMonitoringStore> _stores;
    private readonly ConcurrentStack<Process> _processes = new();

    public AnalyticsManager(IEnumerable<IMonitoringStore> stores)
    {
        _stores = stores;
    }

    public Process GetProcess()
    {
        _processes.TryPeek(out var process);
        return process;
    }

    public void MarkProcessDisabled()
    {
        var process = GetProcess();
        if (process != null)
        {
            process.Disabled = true;
        }
    }

    public void MarkProcessInError()
    {
        var process = GetProcess();
        if (process != null)
        {
            process.Error = true;
        }
    }

    public void StartProcess(string name, string category, string target = null)
    {
        var process = new Process();
        _processes.TryPeek(out var parentProcess);
        process.Disabled = parentProcess?.Disabled ?? false;
        _processes.Push(process);

        foreach (var store in _stores)
        {
            store.StartProcess(process.Id, name, category, target);
        }
    }

    public Process StopProcess()
    {
        _processes.TryPop(out var process);
        process.EndTime = DateTime.Now;

        foreach (var store in _stores)
        {
            store.StopProcess(process.Id, !process.Error, process.Disabled);
        }

        return process;
    }
}
