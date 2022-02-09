using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Kinetix.Monitoring.Memory
{
    public static class MonitoringConfigExtensions
    {
        public static MonitoringConfig AddMemory(this MonitoringConfig config, TimeSpan? expirationDelay = null)
        {
            return config.AddStore(p => new MemoryMonitoringStore(p.GetService<IMemoryCache>(), expirationDelay ?? TimeSpan.FromHours(1)));
        }
    }
}
