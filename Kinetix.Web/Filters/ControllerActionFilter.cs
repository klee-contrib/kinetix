using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Kinetix.Web.Filters;

/// <summary>
/// Enregistre le nom du contrôleur et de l'action dans l'activité OpenTelemetry associée à la requête.
/// </summary>
public class ControllerActionFilter : IEndpointFilter
{
    public const string TagName = "aspnet.controller.action";

    /// <inheritdoc cref="IEndpointFilter.InvokeAsync" />
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var actionDescriptor = context.HttpContext.GetEndpoint()?.Metadata?.GetMetadata<ControllerActionDescriptor>();
        if (actionDescriptor != null)
        {
            var pathParams = actionDescriptor
                .Parameters.Where(p => p.BindingInfo?.BindingSource == BindingSource.Path)
                .Select(p => p.Name);
            Activity.Current?.SetTag(
                TagName,
                $"{actionDescriptor.ControllerName}/{actionDescriptor.ActionName}{(pathParams.Any() ? $" [{string.Join(", ", pathParams)}]" : string.Empty)}"
            );
        }

        return await next(context);
    }
}
