using System.Reflection;
using Kinetix.ComponentModel.Exceptions;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Kinetix.Web.Filters;

/// <summary>
/// Filtre pour gérer les exceptions.
/// </summary>
public class ExceptionFilter : IExceptionFilter
{
    private readonly TelemetryClient _telemetryClient;

    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="telemetryClient">Composant injecté.</param>
    public ExceptionFilter(TelemetryClient telemetryClient)
    {
        _telemetryClient = telemetryClient;
    }

    /// <summary>
    /// A chaque exception.
    /// </summary>
    /// <param name="context">Contexte de l'exception.</param>
    public void OnException(ExceptionContext context)
    {
        var msg = GetExceptionMessage(context.Exception);
        if (msg != null)
        {
            context.Result = msg;
        }

        _telemetryClient.TrackException(context.Exception);
    }

    private IActionResult GetExceptionMessage(Exception exception)
    {
        exception = exception is TargetInvocationException tex ? tex.InnerException : exception;
        return exception switch
        {
            BusinessException ce => BusinessExceptionHandler(ce),
            InvalidOperationException { Source: "Microsoft.EntityFrameworkCore" } => MissingEntityException(),
            _ => DefaultExceptionHandler(exception),
        };
    }

    private IActionResult BusinessExceptionHandler(BusinessException ex)
    {
        var errorDico = new Dictionary<string, object>();
        if (ex.Errors != null && ex.Errors.HasError)
        {
            foreach (var error in ex.Errors)
            {
                if (string.IsNullOrEmpty(error.FieldName))
                {
                    if (!errorDico.ContainsKey(EntityException.GlobalErrorKey))
                    {
                        errorDico.Add(EntityException.GlobalErrorKey, new List<string>());
                    }

                    ((ICollection<string>)errorDico[EntityException.GlobalErrorKey]).Add(error.Message);
                }
                else
                {
                    errorDico.Add(error.FieldName, error.Message);
                }
            }
        }

        if (!string.IsNullOrEmpty(ex.BaseMessage))
        {
            if (!errorDico.ContainsKey(EntityException.GlobalErrorKey))
            {
                errorDico.Add(EntityException.GlobalErrorKey, new List<string> { ex.BaseMessage });
            }
        }

        if (ex.Code != null)
        {
            errorDico.Add(EntityException.CodeKey, ex.Code);
        }

        return new ObjectResult(errorDico) { StatusCode = 400 };
    }

    private IActionResult DefaultExceptionHandler(Exception ex)
    {
        var errors = new List<string> { ex.Message };
        var errorDico = new Dictionary<string, object>
        {
            [EntityException.GlobalErrorKey] = errors
        };

        while (ex.InnerException != null)
        {
            ex = ex.InnerException;
            errors.Add(ex.Message);
        }

        return new ObjectResult(errorDico) { StatusCode = 500 };
    }

    private IActionResult MissingEntityException()
    {
        var errorDico = new Dictionary<string, object>
        {
            [EntityException.GlobalErrorKey] = new List<string> { "L'objet demandé n'existe pas." },
        };

        return new ObjectResult(errorDico) { StatusCode = 404 };
    }
}
