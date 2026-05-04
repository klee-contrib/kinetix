using System.Diagnostics;
using System.Reflection;
using Kinetix.Services.DependencyInjection.Interceptors;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Kinetix.Web.Exceptions;

/// <summary>
/// Handler d'exception pour Kinetix.
/// </summary>
internal class KinetixExceptionHandler(KinetixExceptionConfig config, ProblemDetailsFactory problemDetailsFactory)
    : IExceptionHandler
{
    /// <inheritdoc cref="IExceptionHandler.TryHandleAsync" />
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        while (exception is TargetInvocationException || exception is InterceptedException)
        {
            exception = exception switch
            {
                TargetInvocationException tex => tex.InnerException!,
                InterceptedException iex => iex.InnerException!,
                _ => exception,
            };
        }

        IResult? result = null;

        foreach (
            var exceptionHandler in httpContext
                .RequestServices.GetRequiredService<IEnumerable<IKinetixExceptionHandler>>()
                .OrderByDescending(eh => eh.Priority)
        )
        {
            result = await exceptionHandler.Handle(exception, httpContext, cancellationToken);
            if (result != null)
            {
                break;
            }
        }

        result ??= DefaultExceptionHandler(exception, httpContext);

        Activity.Current?.AddException(exception);
        Activity.Current?.SetStatus(ActivityStatusCode.Error);

        await result.ExecuteAsync(httpContext);

        return true;
    }

    private IResult DefaultExceptionHandler(Exception ex, HttpContext httpContext)
    {
        var errors = new List<string> { ex.Message };

        while (ex.InnerException != null)
        {
            ex = ex.InnerException;
            errors.Add(ex.Message);
        }

        var statusCode = ex switch
        {
            BadHttpRequestException br => br.StatusCode,
            _ => 500,
        };

        if (config.Format == KinetixErrorFormat.Kinetix)
        {
            return Results.Json(new KinetixErrorResponse { Errors = errors }, statusCode: statusCode);
        }
        else
        {
            var problemDetails = problemDetailsFactory.CreateProblemDetails(httpContext, statusCode: statusCode);

            problemDetails.Detail = errors[0];

            if (errors.Count > 1)
            {
                problemDetails.Extensions["errors"] = new Dictionary<string, List<string>>
                {
                    ["origin"] = errors.Skip(1).ToList(),
                };
            }

            return Results.Problem(problemDetails);
        }
    }
}
