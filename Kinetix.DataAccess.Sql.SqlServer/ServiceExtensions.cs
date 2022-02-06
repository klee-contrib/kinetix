using Kinetix.DataAccess.Sql.Broker;
using Kinetix.DataAccess.Sql.SqlServer.Broker;
using Microsoft.Extensions.DependencyInjection;

namespace Kinetix.DataAccess.Sql.SqlServer;

/// <summary>
/// Pour enregistrement dans la DI.
/// </summary>
public static class ServiceExtensions
{
    /// <summary>
    /// Enregistre Kinetix.DataAccess.Sql.SqlServer.
    /// </summary>
    /// <param name="services">ServiceCollection.</param>
    /// <param name="builder">Configuration.</param>
    /// <returns>ServiceCollection.</returns>
    public static IServiceCollection AddSqlServer(this IServiceCollection services, Action<SqlConfig> builder)
    {
        return services
            .AddSql(builder)
            .AddSingleton<CommandParser, SqlServerCommandParser>()
            .AddScoped<BrokerManager, SqlServerBrokerManager>()
            .AddScoped<ConnectionPool, SqlServerConnectionPool>();
    }
}
