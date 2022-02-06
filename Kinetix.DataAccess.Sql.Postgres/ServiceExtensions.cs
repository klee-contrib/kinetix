using Kinetix.DataAccess.Sql.Broker;
using Kinetix.DataAccess.Sql.Postgres.Broker;
using Microsoft.Extensions.DependencyInjection;

namespace Kinetix.DataAccess.Sql.Postgres;

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
    public static IServiceCollection AddPostgres(this IServiceCollection services, Action<SqlConfig> builder)
    {
        return services
            .AddSql(builder)
            .AddSingleton<CommandParser, PostgresCommandParser>()
            .AddScoped<BrokerManager, PostgresBrokerManager>()
            .AddScoped<ConnectionPool, PostgresConnectionPool>();
    }
}
