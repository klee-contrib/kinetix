using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;

namespace KinetixCore.Monitoring
{
    public class AnalyticsActionFilter : IActionFilter
    {
        private readonly IAnalyticsManager _analyticsManager;

        public AnalyticsActionFilter(IAnalyticsManager analyticsManager)
        {
            _analyticsManager = analyticsManager;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            string actionName = context.RouteData.Values["action"].ToString();
            string controllerName = context.RouteData.Values["controller"].ToString();

            _analyticsManager.BeginTrace("webservices", "/" + context.HttpContext.Request.Method + "/" + controllerName + "/" + actionName, (tracer) => { tracer.AddTag("path", context.HttpContext.Request.Path.ToString()); });
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            _analyticsManager.EndTraceSuccess((tracer) => { });
        }

    }
}
