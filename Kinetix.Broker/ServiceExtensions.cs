using System;
using System.Reflection;
using System.Transactions;
using Kinetix.Data.SqlClient;
using Kinetix.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Kinetix.Broker
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddBroker(this IServiceCollection services, Action<SqlServerConfig> builder)
        {
            var config = new SqlServerConfig();
            builder(config);

            // Pour override le timeout par défaut à 10min.
            SetTransactionManagerField("s_cachedMaxTimeout", true);
            SetTransactionManagerField("s_maximumTimeout", TimeSpan.FromHours(1));

            return services
                .AddSingleton(config)
                .AddSingleton<SqlServerManager>()
                .AddSingleton<CommandParser>()
                .AddSingleton<ITransactionContextProvider, SqlTransactionContextProvider>()
                .AddScoped<ConnectionPool>()
                .AddScoped<BrokerManager>();
        }

        static void SetTransactionManagerField(string fieldName, object value)
        {
            typeof(TransactionManager)
                .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static)
                .SetValue(null, value);
        }
    }
}
