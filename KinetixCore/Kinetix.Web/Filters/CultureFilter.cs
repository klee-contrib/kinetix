using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;

namespace Kinetix.Web.Filters
{
    /// <summary>
    /// Filtre pour gérer la culture dans la Web API.
    /// </summary>
    public class CultureFilter : IActionFilter
    {
        /// <summary>
        /// Nom de l'entête HTTP contenant le code de la culture.
        /// </summary>
        private const string CultureHeaderCode = "CultureCode";

        /// <summary>
        /// Code de la culture par défaut.
        /// </summary>
        private const string DefaultCultureCode = "fr-FR";

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // RAS.
        }

        /// <summary>
        /// Action.
        /// </summary>
        /// <param name="context">Current context.</param>
        public void OnActionExecuting(ActionExecutingContext context)
        {
            context.HttpContext.Request.Headers.TryGetValue(CultureHeaderCode, out var cultureList);

            if (cultureList == default(StringValues))
            {
                cultureList = new StringValues(DefaultCultureCode);
            }

            if (cultureList.Count() == 1)
            {
                var cultureCode = cultureList.First();
                Thread.CurrentThread.CurrentCulture = new CultureInfo(cultureCode);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureCode);
            }
            else if (cultureList.Count() > 1)
            {
                throw new NotSupportedException("Too many CultureCode defined in client request.");
            }
        }
    }
}