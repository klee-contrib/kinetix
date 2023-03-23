using System.Security;
using Kinetix.Modeling.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Kinetix.Web.Exceptions;

/// <summary>
/// Handler par défaut pour les SecurityException
/// </summary>
public class SecurityExceptionHandler : IExceptionHandler
{
    /// <inheritdoc />
    public int Priority => 1;

    /// <inheritdoc cref="IExceptionHandler.Handle" />
    public IResult Handle(Exception exception)
    {
        if (exception is not SecurityException)
        {
            return null;
        }

        return Results.Json(
            new Dictionary<string, List<string>>
            {
                [EntityException.GlobalErrorKey] = new() { exception.Message },
            },
            statusCode: 403);
    }
}
