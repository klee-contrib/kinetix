using System;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace Kinetix.Monitoring.Insights
{
    /// <summary>
    /// Vire la télémétrie AI des dépendances sans opération parente (ex : ping des batchs).
    /// </summary>
    public class MonitoringTelemetryFilter : ITelemetryProcessor
    {
        private readonly ITelemetryProcessor _next;

        /// <summary>
        /// Constructeur.
        /// </summary>
        /// <param name="next">Processeur suivant.</param>
        public MonitoringTelemetryFilter(ITelemetryProcessor next)
        {
            _next = next;
        }

        /// <inheritdoc cref="ITelemetryProcessor.Process" />
        public void Process(ITelemetry item)
        {
            // Vire toute la télémétrie tagguée "disabled".
            if (item is ISupportProperties t && t.Properties.TryGetValue("disabled", out var _))
            {
                return;
            }

            if (item is DependencyTelemetry dt)
            {
                if (dt.Type == "SQL")
                {
                    // Géré par Kinetix.Data.SqlClient déjà.
                    return;
                }

                if (dt.Duration < TimeSpan.FromMilliseconds(10) && dt.Success == true)
                {
                    // C'est du bruit on vire.
                    return;
                }

                dt.Type = dt.Type == "Service" ? "InProc" : dt.Type == "Database" ? "SQL" : dt.Type;
            }

            // Filtre les diagnostics.
            if (item is RequestTelemetry rt && rt.Name.Contains("GET") && (rt.Name.Contains("Ws/Local/Edition.svc") || rt.Name.Contains("ExecuteDiagnostic")))
            {
                return;
            }

            // Filtre les exceptions depuis le middleware Analytics (elles sont déjà remontées par ailleurs).
            if (item is ExceptionTelemetry et && et.Properties.TryGetValue("CategoryName", out var category))
            {
                if (category == "Kinetix.Services.Service")
                {
                    return;
                }
            }

            _next.Process(item);
        }
    }
}
