using Kinetix.Modeling.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Kinetix.Web.Exceptions;

/// <summary>
/// Handler par défaut pour les "Single" en erreur dans EF Core.
/// </summary>
public class MissingEntityExceptionHandler : IExceptionHandler
{
    /// <inheritdoc />
    public int Priority => 1;

    /// <inheritdoc cref="IExceptionHandler.Handle" />
    public IResult Handle(Exception exception)
    {
        if (exception is not InvalidOperationException { Source: "Microsoft.EntityFrameworkCore" } ioe)
        {
            return null;
        }

        return Results.NotFound(new Dictionary<string, List<string>>
        {
            [EntityException.GlobalErrorKey] = new() { "L'objet demandé n'existe pas." },
        });
    }
}
