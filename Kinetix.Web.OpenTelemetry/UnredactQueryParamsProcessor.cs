using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using OpenTelemetry;

namespace Kinetix.Web.OpenTelemetry;

/// <summary>
/// Un processeur qui annule l'obfuscation des paramètres de la query dans l'URL par OpenTelemetry.
/// </summary>
public class UnredactQueryParamsProcessor(IHttpContextAccessor httpContextAccessor) : BaseProcessor<Activity>
{
    /// <inheritdoc/>
    public override void OnEnd(Activity activity)
    {
        var ctx = httpContextAccessor.HttpContext;
        if (ctx is not null && activity.Kind == ActivityKind.Server)
        {
            var originalUrl = ctx.Request.GetDisplayUrl();

            if (ctx.Request.QueryString.HasValue)
            {
                activity.SetTag("url.query", ctx.Request.QueryString);
            }

            activity.SetTag("http.target", originalUrl);
        }
    }
}
