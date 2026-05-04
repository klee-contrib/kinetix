using System.Diagnostics;
using Kinetix.Web.Filters;
using OpenTelemetry;

namespace Kinetix.Web;

/// <summary>
/// Processeur pour appliquer des filtres de base sur les traces OpenTelemetry.
/// </summary>
/// <remarks>Ce processeur s'occupe aussi de la propagation du tag d'action du contrôleur.</remarks>
public class ActivityFilterProcessor : BaseProcessor<Activity>
{
    /// <summary>
    /// Durée minimale d'une dépendance en succès pour être conservée.
    /// </summary>
    public long MinDependencyDuration { get; set; } = 10L;

    /// <summary>
    /// Routes à retirer des traces.
    /// </summary>
    public IList<(HttpMethod Method, string Route)> FilteredRoutes { get; } = [];

    /// <summary>
    /// Enregistre des routes en GET à retirer des traces.
    /// </summary>
    /// <param name="routes">Les routes.</param>
    public void AddFilteredGetRoutes(params IEnumerable<string> routes)
    {
        foreach (var route in routes)
        {
            FilteredRoutes.Add((HttpMethod.Get, route));
        }
    }

    /// <summary>
    /// Enregistre des routes avec une méthode HTTP spécifique à retirer des traces.
    /// </summary>
    /// <param name="routes">Les routes.</param>
    public void AddFilteredRoutes(params IEnumerable<(HttpMethod Method, string Route)> routes)
    {
        foreach (var route in routes)
        {
            FilteredRoutes.Add(route);
        }
    }

    /// <inheritdoc />
    public override void OnEnd(Activity data)
    {
        if (
            data.Kind != ActivityKind.Server
            && data.Duration <= TimeSpan.FromMilliseconds(MinDependencyDuration)
            && data.Status != ActivityStatusCode.Error
        )
        {
            data.ActivityTraceFlags &= ~ActivityTraceFlags.Recorded;
        }

        if (
            data.Kind == ActivityKind.Server
            && FilteredRoutes.Any(route =>
                data.DisplayName.StartsWith(
                    $"{route.Method.Method} {route.Route}",
                    StringComparison.InvariantCultureIgnoreCase
                )
            )
        )
        {
            data.ActivityTraceFlags &= ~ActivityTraceFlags.Recorded;
        }

        // Permet d'ajuster l'affichage des traces de Npgsql pour qu'elles apparaissent proprement dans AppInsights.
        if (data.GetTagItem("db.system.name") as string == "postgresql")
        {
            data.SetTag("db.system.name", "sql");

            var dbName = data.GetTagItem("db.namespace")?.ToString();
            data.SetTag("db.namespace", value: null);

            var serverAddress = data.GetTagItem("server.address")?.ToString();
            if (serverAddress != null)
            {
                data.SetTag(
                    "server.address",
                    $"{dbName}@{serverAddress.Replace(".postgres.database.azure.com", string.Empty)}"
                );
            }
        }
    }

    /// <inheritdoc />
    public override void OnStart(Activity activity)
    {
        // Propage le tag de l'action du contrôleur sur les dépendances enfantes.
        var actionTag = activity.Parent?.Tags.FirstOrDefault(t => t.Key == ControllerActionFilter.TagName).Value;
        if (actionTag != null)
        {
            activity.SetTag(ControllerActionFilter.TagName, actionTag);
        }
    }
}
