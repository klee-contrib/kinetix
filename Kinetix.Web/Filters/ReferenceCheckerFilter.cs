using Kinetix.Services;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Kinetix.Web.Filters
{
    public class ReferenceCheckerFilter : IActionFilter
    {
        private readonly IReferenceManager _referenceManager;

        public ReferenceCheckerFilter(IReferenceManager referenceManager)
        {
            _referenceManager = referenceManager;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            foreach (var parameter in context.ActionArguments)
            {
                _referenceManager.CheckReferenceKeys(parameter.Value);
            }
        }
    }
}
