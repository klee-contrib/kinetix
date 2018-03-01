using System;
using System.Collections.Generic;
using Kinetix.ComponentModel.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Kinetix.Web.Filters
{
    /// <summary>
    /// Filtre pour gérer les exceptions.
    /// </summary>
    public class ExceptionFilter : IExceptionFilter
    {
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
        }

        private IActionResult GetExceptionMessage(Exception exception)
        {
            switch (exception.GetType().Name)
            {
                case "EntityException":
                    return EntityExceptionHandler(exception as EntityException);
                case "ConstraintException":
                    return ConstraintExceptionHandler(exception as ConstraintException);
                default:
                    return DefaultExceptionHandler(exception);
            }
        }

        private IActionResult ConstraintExceptionHandler(ConstraintException ex)
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

            if (!string.IsNullOrEmpty(ex.Message))
            {
                if (!errorDico.ContainsKey(EntityException.GlobalErrorKey))
                {
                    errorDico.Add(EntityException.GlobalErrorKey, new List<string> { ex.Message });
                }
            }

            if (ex.Code != null)
            {
                errorDico.Add(EntityException.CodeKey, ex.Code);
            }

            return new ObjectResult(errorDico) { StatusCode = 400 };
        }

        /// <summary>
        /// Security exception handler method.
        /// </summary>
        /// <param name="ex">Current exception.</param>
        /// <returns>Message.</returns>
        private IActionResult DefaultExceptionHandler(Exception ex)
        {
            var errorDico = new Dictionary<string, object> { { EntityException.GlobalErrorKey, new List<string> { ex.Message } } };
            return new ObjectResult(errorDico) { StatusCode = 500 };
        }

        /// <summary>
        /// Default execption handler.
        /// </summary>
        /// <param name="ex">Exception to handle.</param>
        /// <returns>Http response.</returns>
        private IActionResult EntityExceptionHandler(EntityException ex)
        {
            ex.AddError("type", "entity");
            return new ObjectResult(ex.ErrorList) { StatusCode = 400 };
        }
    }
}