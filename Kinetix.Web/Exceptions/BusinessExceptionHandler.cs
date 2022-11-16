using Kinetix.Modeling.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Kinetix.Web.Exceptions;

/// <summary>
/// Handler par défaut pour les BusinessException.
/// </summary>
public class BusinessExceptionHandler : IExceptionHandler
{
    /// <inheritdoc />
    public int Priority => 1;

    /// <inheritdoc cref="IExceptionHandler.Handle" />
    public IResult Handle(Exception exception)
    {
        if (exception is not BusinessException be)
        {
            return null;
        }

        var result = new Dictionary<string, object>();
        var errors = new List<string>();

        if (be.Errors != null && be.Errors.HasError)
        {
            foreach (var error in be.Errors)
            {
                if (string.IsNullOrEmpty(error.FieldName))
                {
                    errors.Add(error.Message);
                }
                else
                {
                    result.Add(error.FieldName, error.Message);
                }
            }
        }

        if (!string.IsNullOrEmpty(be.BaseMessage))
        {
            errors.Add(be.BaseMessage);
        }

        if (errors.Any())
        {
            result.Add(EntityException.GlobalErrorKey, errors);
        }

        if (be.Code != null)
        {
            result.Add(EntityException.CodeKey, be.Code);
        }

        return Results.BadRequest(result);
    }
}
