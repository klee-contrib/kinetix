using Kinetix.Monitoring.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace KinetixCore.Monitoring
{
    public class AnalyticsActionFilter : IActionFilter
    {
        private readonly IAnalyticsManager _analyticsManager;

        /// <summary>
        /// Default constructor for AnalyticsActionFilter
        /// </summary>
        /// <param name="analyticsManager">analyticsManager</param>
        public AnalyticsActionFilter(IAnalyticsManager analyticsManager)
        {
            _analyticsManager = analyticsManager;
        }

        /// <summary>
        /// Monitoring Hook method Before executing MVC controller/action
        /// </summary>
        /// <param name="context"></param>
        public void OnActionExecuting(ActionExecutingContext context)
        {
            string actionName = context.RouteData.Values["action"].ToString();
            string controllerName = context.RouteData.Values["controller"].ToString();

            _analyticsManager.BeginTrace("webservices", $"/{context.HttpContext.Request.Method}/{controllerName}/{actionName}", (tracer) => { tracer.AddTag("path", context.HttpContext.Request.Path.ToString()); });
        }

        /// <summary>
        /// Monitoring Hook method After executing MVC controller/action
        /// </summary>
        /// <param name="context"></param>
        public void OnActionExecuted(ActionExecutedContext context)
        {
            _analyticsManager.EndTraceSuccess((tracer) => { });
        }

    }
}
