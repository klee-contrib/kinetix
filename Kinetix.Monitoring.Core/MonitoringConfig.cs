using Microsoft.Extensions.DependencyInjection;

namespace Kinetix.Monitoring.Core;

public class MonitoringConfig(IServiceCollection services)
{
    public MonitoringConfig AddStore<T>(Func<IServiceProvider, T> store)
        where T : class, IMonitoringStore
    {
        services.AddSingleton<IMonitoringStore>(store);
        return this;
    }
}
