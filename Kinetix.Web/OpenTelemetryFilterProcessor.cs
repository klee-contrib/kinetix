using System.Diagnostics;
using OpenTelemetry;

namespace Kinetix.Web;

/// <summary>
/// Filtre par défaut pour OpenTelemetry.
/// </summary>
/// <param name="filteredRoutes">Routes à filtrer.</param>
public class OpenTelemetryFilterProcessor(params IEnumerable<string> filteredRoutes) : BaseProcessor<Activity>
{
    /// <inheritdoc />
    public override void OnEnd(Activity data)
    {
        // Retire les activités hors serveur (= requêtes entrantes) en succès de moins de 10ms.
        if (
            data.Kind != ActivityKind.Server
            && data.Duration <= TimeSpan.FromMilliseconds(10)
            && data.Status != ActivityStatusCode.Error
        )
        {
            data.ActivityTraceFlags &= ~ActivityTraceFlags.Recorded;
        }

        if (
            data.Kind == ActivityKind.Server
            && filteredRoutes.Any(route =>
                data.DisplayName.StartsWith(route, StringComparison.InvariantCultureIgnoreCase)
            )
        )
        {
            data.ActivityTraceFlags &= ~ActivityTraceFlags.Recorded;
        }

        if (data.GetTagItem("db.name") != null)
        {
            data.AddTag("az.namespace", "sql");
        }
    }
}
