using System;
using Kinetix.Data.SqlClient;
using Kinetix.Monitoring;
using Microsoft.Extensions.DependencyInjection;

namespace Kinetix.Broker
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddBroker(this IServiceCollection services, Action<SqlServerConfig> builder)
        {
            var config = new SqlServerConfig();
            builder(config);

            return services
                .AddSingleton(config)
                .AddSingleton<SqlServerManager>()
                .AddSingleton<CommandParser>()
                .AddScoped<SqlServerAnalytics>()
                .AddScoped<IAnalytics, SqlServerAnalytics>(p => p.GetService<SqlServerAnalytics>())
                .AddScoped<ConnectionPool>()
                .AddScoped<BrokerManager>();
        }
    }
}
