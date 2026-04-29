using System.Text.RegularExpressions;
using Npgsql;

namespace Kinetix.EFCore.Npgsql;

public static class NpgsqlDataSourceBuilderExtensions
{
    /// <summary>
    /// Configure les traces de Npgsql pour qu'elles apparaissent dans OpenTelemetry (à priori Application Insights).
    /// </summary>
    /// <param name="builder">DataSourceBuilder.</param>
    /// <returns>DataSourceBuilder.</returns>
    public static NpgsqlDataSourceBuilder ConfigureOpenTelemetry(this NpgsqlDataSourceBuilder builder)
    {
        return builder.ConfigureTracing(e =>
            e.EnableFirstResponseEvent(false)
                .ConfigureCommandFilter(cmd =>
                    !cmd.CommandText.Contains("SAVEPOINT", StringComparison.OrdinalIgnoreCase)
                )
                .ConfigureCommandSpanNameProvider(e =>
                {
                    var queryMatch = Regex.Match(
                        e.CommandText,
                        @"(SELECT|INSERT|UPDATE|DELETE)(.+\n?FROM| INTO)? (\w+)"
                    );
                    return queryMatch.Groups.Count == 4
                        ? $"{queryMatch.Groups[1].Value} {queryMatch.Groups[3].Value}"
                        : "query";
                })
        );
    }
}
