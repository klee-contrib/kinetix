using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;

namespace Kinetix.Monitoring.Memory
{
    public class MemoryMonitoringStore : IMonitoringStore
    {
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _expirationDelay;

        public MemoryMonitoringStore(IMemoryCache cache, TimeSpan expirationDelay)
        {
            _cache = cache;
            _expirationDelay = expirationDelay;
        }

        private ConcurrentBag<Process> ProcessCache => _cache.GetOrCreate("Analytics", cacheEntry =>
        {
            cacheEntry.AbsoluteExpirationRelativeToNow = _expirationDelay;
            return new ConcurrentBag<Process>();
        });

        public void AddProcess(Process process)
        {
            ProcessCache.Add(process);
        }

        public Process GetProcess(Guid id)
        {
            return ProcessCache.Single(p => p.Id == id);
        }

        public IEnumerable<ProcessSummary> GetProcessSummaryByCategory(string category, SortOrder sortOrder)
        {
            return ProcessCache.Where(p => p.Category == category)
                .GroupBy(p => p.Name)
                .Select(g => new ProcessSummary
                {
                    Name = g.Key,
                    Count = g.Count(),
                    TotalDuration = g.Sum(p => p.Duration.Value),
                    MeanDuration = Convert.ToInt32(g.Average(p => p.Duration.Value)),
                    MaxDuration = g.Max(p => p.Duration.Value),
                    Processes = g.Select(p => p.Id)
                })
                .OrderByDescending(p => sortOrder switch
                {
                    SortOrder.Count => p.Count,
                    SortOrder.TotalDuration => p.TotalDuration,
                    SortOrder.MeanDuration => p.MeanDuration,
                    SortOrder.MaxDuration => p.MaxDuration,
                    _ => 0
                }); ;
        }
    }
}
