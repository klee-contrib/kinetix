using System;
using Kinetix.Data.SqlClient;
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
                .AddScoped<ConnectionPool>()
                .AddScoped<BrokerManager>();
        }
    }
}
