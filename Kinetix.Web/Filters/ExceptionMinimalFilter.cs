using Kinetix.Web.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Kinetix.Web.Filters;

/// <summary>
/// Filtre pour gérer les exceptions.
/// </summary>
public class ExceptionMinimalFilter : IEndpointFilter
{
    /// <inheritdoc cref="IEndpointFilter.InvokeAsync" />
    public async ValueTask<object> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        try
        {
            return await next(context);
        }
        catch (Exception ex)
        {
            return ExceptionHandler.Handle(ex, context.HttpContext.RequestServices);
        }
    }
}
