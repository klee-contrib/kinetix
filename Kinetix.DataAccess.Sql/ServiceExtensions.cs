using System.Reflection;
using System.Transactions;
using Kinetix.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Kinetix.DataAccess.Sql;

/// <summary>
/// Pour enregistrement dans la DI.
/// </summary>
public static class ServiceExtensions
{
    /// <summary>
    /// Enregistre Kinetix.DataAccess.Sql.
    /// </summary>
    /// <param name="services">ServiceCollection.</param>
    /// <param name="builder">Configuration.</param>
    /// <returns>ServiceCollection.</returns>
    public static IServiceCollection AddSql(this IServiceCollection services, Action<SqlConfig> builder)
    {
        var config = new SqlConfig();
        builder(config);

        // Pour override le timeout par défaut à 10min.
        SetTransactionManagerField("s_cachedMaxTimeout", true);
        SetTransactionManagerField("s_maximumTimeout", TimeSpan.FromHours(1));

        return services
            .AddSingleton(config)
            .AddSingleton<SqlManager>()
            .AddSingleton<ITransactionContextProvider, SqlTransactionContextProvider>();
    }

    /// <summary>
    /// Met à jour un champ privé du TransactionManager.
    /// </summary>
    /// <param name="fieldName">Nom du champ.</param>
    /// <param name="value">Valeur.</param>
    private static void SetTransactionManagerField(string fieldName, object value)
    {
        typeof(TransactionManager)
            .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static)
            .SetValue(null, value);
    }
}
