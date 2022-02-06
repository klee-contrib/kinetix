using Microsoft.Extensions.DependencyInjection;

namespace Kinetix.Monitoring;

public class MonitoringConfig
{
    private readonly IServiceCollection _services;

    public MonitoringConfig(IServiceCollection services)
    {
        _services = services;
    }

    public MonitoringConfig AddStore<T>(Func<IServiceProvider, T> store)
        where T : class, IMonitoringStore
    {
        _services.AddSingleton<IMonitoringStore>(store);
        return this;
    }
}
