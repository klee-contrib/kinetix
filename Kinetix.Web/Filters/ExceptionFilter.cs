using Kinetix.Web.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Kinetix.Web.Filters;

/// <summary>
/// Filtre pour gérer les exceptions.
/// </summary>
public class ExceptionFilter : IExceptionFilter
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="serviceProvider">Composant injecté.</param>
    public ExceptionFilter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// A chaque exception.
    /// </summary>
    /// <param name="context">Contexte de l'exception.</param>
    public void OnException(ExceptionContext context)
    {
        var msg = ExceptionHandler.Handle(context.Exception, _serviceProvider);
        if (msg != null)
        {
            context.Result = new HttpActionResult(msg);
        }
    }

    private class HttpActionResult : ActionResult
    {
        /// <summary>
        /// Gets the instance of the current <see cref="IResult"/>.
        /// </summary>
        public IResult Result { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpActionResult"/> class with the
        /// <see cref="IResult"/> provided.
        /// </summary>
        /// <param name="result">The <see cref="IResult"/> instance to be used during the <see cref="ExecuteResultAsync"/> invocation.</param>
        public HttpActionResult(IResult result)
        {
            Result = result;
        }

        /// <inheritdoc/>
        public override Task ExecuteResultAsync(ActionContext context)
            => Result.ExecuteAsync(context.HttpContext);
    }
}
