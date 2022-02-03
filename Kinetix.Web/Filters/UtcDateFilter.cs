using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Kinetix.Web.Filters
{
    public class UtcDateFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var fixedDates = new Dictionary<string, DateTime>();

            foreach (var parameter in context.ActionArguments)
            {
                if (parameter.Value is DateTime date)
                {
                    fixedDates[parameter.Key] = DateTime.SpecifyKind(date, DateTimeKind.Utc);
                }
            }

            foreach (var fixedDate in fixedDates)
            {
                context.ActionArguments[fixedDate.Key] = fixedDate.Value;
            }
        }
    }
}
