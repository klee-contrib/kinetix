using System.Reflection;
using Kinetix.Modeling.Exceptions;
using Kinetix.Services.DependencyInjection.Interceptors;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Kinetix.Web.Exceptions;

/// <summary>
/// Handler d'exception.
/// </summary>
public static class ExceptionHandler
{
    /// <summary>
    /// Gère une exception pour retour d'API.
    /// </summary>
    /// <param name="exception">Exception.</param>
    /// <param name="provider">Service provider.</param>
    /// <returns>Le résultat.</returns>
    public static IResult Handle(Exception exception, IServiceProvider provider)
    {
        while (exception is TargetInvocationException || exception is InterceptedException)
        {
            exception = exception switch
            {
                TargetInvocationException tex => tex.InnerException,
                InterceptedException iex => iex.InnerException,
                _ => exception
            };
        }

        provider.GetService<TelemetryClient>()?.TrackException(exception);

        foreach (var exceptionHandler in provider.GetRequiredService<IEnumerable<IExceptionHandler>>().OrderByDescending(eh => eh.Priority))
        {
            var result = exceptionHandler.Handle(exception);
            if (result != null)
            {
                return result;
            }
        }

        return DefaultExceptionHandler(exception);
    }

    private static IResult DefaultExceptionHandler(Exception ex)
    {
        var errors = new List<string> { ex.Message };

        while (ex.InnerException != null)
        {
            ex = ex.InnerException;
            errors.Add(ex.Message);
        }

        return Results.Json(new Dictionary<string, List<string>> { [EntityException.GlobalErrorKey] = errors }, statusCode: 500);
    }
}
