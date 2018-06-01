using System.Threading.Tasks;
using Kinetix.Monitoring.Abstractions;
using Microsoft.AspNetCore.Http;

namespace KinetixCore.Monitoring
{
    public class AnalyticsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IAnalyticsManager _analyticsManager;

        public AnalyticsMiddleware(RequestDelegate next, IAnalyticsManager analyticsManager)
        {
            _next = next;
            _analyticsManager = analyticsManager;
        }

        public Task Invoke(HttpContext context)
        {
            string name = "/" + context.Request.Method + "/" + context.Request.Path;
            Task ret = null;
            _analyticsManager.Trace("urls", name, tracer =>
            {
                ret = this._next(context);
            });
            return ret;
        }
    }
}
